using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// Append-only enterprise audit service.
/// </summary>
public sealed class EnterpriseAuditService : IEnterpriseAuditService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly ISecurityNotificationService notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnterpriseAuditService"/> class.
    /// </summary>
    public EnterpriseAuditService(
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ISecurityNotificationService notificationService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task AppendAsync(EnterpriseAuditEntry entry, CancellationToken cancellationToken = default)
    {
        var entity = new AuditEvent
        {
            Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id,
            OrganizationId = entry.OrganizationId,
            UserId = entry.UserId,
            ActorEmail = entry.ActorEmail,
            Category = entry.Category,
            EventType = entry.EventType,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Summary = entry.Summary,
            MetadataJson = entry.MetadataJson,
            IpAddress = entry.IpAddress,
            CorrelationId = entry.CorrelationId,
            OccurredAt = entry.OccurredAt == default ? dateTimeService.UtcNow : entry.OccurredAt,
            IsImmutable = true,
        };

        await dbContext.AddAuditEventAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (entity.OrganizationId.HasValue)
        {
            await notificationService.NotifyAuditEventAsync(
                entity.OrganizationId.Value,
                entity.EventType,
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnterpriseAuditEntry>> QueryAsync(
        Guid organizationId,
        AuditQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var q = dbContext.AuditEvents.AsNoTracking()
            .Where(e => e.OrganizationId == organizationId);

        if (query.Category.HasValue)
        {
            q = q.Where(e => e.Category == query.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            q = q.Where(e => e.EventType == query.EventType);
        }

        if (query.FromUtc.HasValue)
        {
            q = q.Where(e => e.OccurredAt >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            q = q.Where(e => e.OccurredAt <= query.ToUtc.Value);
        }

        var take = query.Take <= 0 ? 100 : Math.Min(query.Take, 500);
        var rows = await q.OrderByDescending(e => e.OccurredAt).Take(take).ToListAsync(cancellationToken);
        return rows.Select(e => new EnterpriseAuditEntry
        {
            Id = e.Id,
            OrganizationId = e.OrganizationId,
            UserId = e.UserId,
            ActorEmail = e.ActorEmail,
            Category = e.Category,
            EventType = e.EventType,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            Summary = e.Summary,
            MetadataJson = e.MetadataJson,
            IpAddress = e.IpAddress,
            CorrelationId = e.CorrelationId,
            OccurredAt = e.OccurredAt,
        }).ToList();
    }
}

/// <summary>
/// Evaluates organization governance and security policies.
/// </summary>
public sealed class PolicyEngine : IPolicyEngine
{
    private readonly IApplicationDbContext dbContext;
    private readonly ISecurityNotificationService notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyEngine"/> class.
    /// </summary>
    public PolicyEngine(IApplicationDbContext dbContext, ISecurityNotificationService notificationService)
    {
        this.dbContext = dbContext;
        this.notificationService = notificationService;
    }

    /// <inheritdoc />
    public Task EnsureProviderAllowedAsync(
        Guid organizationId,
        string providerKind,
        CancellationToken cancellationToken = default) =>
        EnsureInAllowListAsync(organizationId, p => p.AllowedProvidersJson, providerKind, "provider", cancellationToken);

    /// <inheritdoc />
    public Task EnsureModelAllowedAsync(
        Guid organizationId,
        string modelName,
        CancellationToken cancellationToken = default) =>
        EnsureInAllowListAsync(organizationId, p => p.AllowedModelsJson, modelName, "model", cancellationToken);

    /// <inheritdoc />
    public async Task EnsurePodLimitAsync(
        Guid organizationId,
        int proposedRunningPods,
        CancellationToken cancellationToken = default)
    {
        var policy = await GetGovernanceAsync(organizationId, cancellationToken);
        if (policy?.MaximumRunningPods is int max && proposedRunningPods > max)
        {
            await ViolateAsync(organizationId, $"Running pod limit exceeded ({proposedRunningPods}/{max}).", cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task EnsureQueueLimitAsync(
        Guid organizationId,
        int proposedQueueSize,
        CancellationToken cancellationToken = default)
    {
        var policy = await GetGovernanceAsync(organizationId, cancellationToken);
        if (policy?.MaximumQueueSize is int max && proposedQueueSize > max)
        {
            await ViolateAsync(organizationId, $"Queue size limit exceeded ({proposedQueueSize}/{max}).", cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task EnsureIpAllowedAsync(
        Guid organizationId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var security = await dbContext.OrganizationSecurityPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, cancellationToken);
        if (security is null)
        {
            return;
        }

        var allowList = ParseList(security.IpAllowListJson);
        if (allowList.Count == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ipAddress) ||
            !allowList.Contains(ipAddress, StringComparer.OrdinalIgnoreCase))
        {
            await ViolateAsync(organizationId, $"IP address '{ipAddress}' is not allowed.", cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task EnsurePluginAllowedAsync(
        Guid organizationId,
        string packageId,
        CancellationToken cancellationToken = default) =>
        EnsureInAllowListAsync(organizationId, p => p.AllowedPluginsJson, packageId, "plugin", cancellationToken);

    /// <inheritdoc />
    public Task EnsureMcpAllowedAsync(
        Guid organizationId,
        string serverKind,
        CancellationToken cancellationToken = default) =>
        EnsureInAllowListAsync(organizationId, p => p.AllowedMcpServersJson, serverKind, "MCP server", cancellationToken);

    private async Task EnsureInAllowListAsync(
        Guid organizationId,
        Func<OrganizationGovernancePolicy, string> selector,
        string value,
        string label,
        CancellationToken cancellationToken)
    {
        var policy = await GetGovernanceAsync(organizationId, cancellationToken);
        if (policy is null)
        {
            return;
        }

        var allowList = ParseList(selector(policy));
        if (allowList.Count == 0 && policy.EmptyAllowListMeansAllowAll)
        {
            return;
        }

        if (!allowList.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            await ViolateAsync(organizationId, $"{label} '{value}' is not allowed by policy.", cancellationToken);
        }
    }

    private async Task<OrganizationGovernancePolicy?> GetGovernanceAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.OrganizationGovernancePolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, cancellationToken);

    private async Task ViolateAsync(Guid organizationId, string message, CancellationToken cancellationToken)
    {
        await notificationService.NotifyPolicyViolationAsync(organizationId, message, cancellationToken);
        throw new ForbiddenException(message);
    }

    private static IReadOnlyList<string> ParseList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
