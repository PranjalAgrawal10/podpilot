using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// Raises and broadcasts security alerts.
/// </summary>
public sealed class SecurityAlertService : ISecurityAlertService
{
    private readonly IEnterpriseAuditService auditService;
    private readonly ISecurityNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityAlertService"/> class.
    /// </summary>
    public SecurityAlertService(
        IEnterpriseAuditService auditService,
        ISecurityNotificationService notificationService,
        IDateTimeService dateTimeService)
    {
        this.auditService = auditService;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task RaiseAsync(SecurityAlert alert, CancellationToken cancellationToken = default)
    {
        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = alert.OrganizationId,
                UserId = alert.UserId,
                Category = AuditEventCategory.Security,
                EventType = alert.AlertType.ToString(),
                Summary = alert.Message,
                IpAddress = alert.IpAddress,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        await notificationService.NotifySecurityAlertAsync(
            alert.OrganizationId,
            alert.AlertType.ToString(),
            alert.Message,
            cancellationToken);
    }
}

/// <summary>
/// Computes security dashboard metrics.
/// </summary>
public sealed class SecurityDashboardService : ISecurityDashboardService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IEnterpriseAuditService auditService;
    private readonly IComplianceService complianceService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityDashboardService"/> class.
    /// </summary>
    public SecurityDashboardService(
        IApplicationDbContext dbContext,
        IEnterpriseAuditService auditService,
        IComplianceService complianceService,
        IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.complianceService = complianceService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<SecurityDashboard> GetAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var since = dateTimeService.UtcNow.AddHours(-24);
        var activeSessions = await dbContext.SessionHistories.AsNoTracking()
            .CountAsync(s => s.OrganizationId == organizationId && s.IsActive && s.Succeeded, cancellationToken);
        var failedLogins = await dbContext.SessionHistories.AsNoTracking()
            .CountAsync(s => s.OrganizationId == organizationId && !s.Succeeded && s.StartedAt >= since, cancellationToken);
        var secretCount = await dbContext.SecretReferences.AsNoTracking()
            .CountAsync(s => s.OrganizationId == organizationId, cancellationToken);
        var expiring = await dbContext.SecretReferences.AsNoTracking()
            .CountAsync(
                s => s.OrganizationId == organizationId &&
                     s.ExpiresAt != null &&
                     s.ExpiresAt < dateTimeService.UtcNow.AddDays(30),
                cancellationToken);

        var memberIds = await dbContext.OrganizationMembers.AsNoTracking()
            .Where(m => m.OrganizationId == organizationId && m.IsActive)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
        var mfaEnabled = memberIds.Count == 0
            ? 0
            : await dbContext.UserMfaEnrollments.AsNoTracking()
                .CountAsync(e => memberIds.Contains(e.UserId) && e.IsEnabled, cancellationToken);
        var mfaCoverage = memberIds.Count == 0 ? 100 : (double)mfaEnabled / memberIds.Count * 100;

        var audits = await auditService.QueryAsync(
            organizationId,
            new AuditQueryRequest { Take = 10 },
            cancellationToken);
        var compliance = await complianceService.GetStatusAsync(organizationId, cancellationToken);

        var score = 100;
        score -= Math.Min(30, failedLogins * 2);
        score -= expiring > 0 ? 10 : 0;
        score -= mfaCoverage < 50 ? 20 : 0;
        score = Math.Clamp(score, 0, 100);

        return new SecurityDashboard
        {
            SecurityScore = score,
            ActiveSessions = activeSessions,
            FailedLogins24h = failedLogins,
            RecentAuditEvents = audits.Count,
            SecretCount = secretCount,
            ExpiringSecrets = expiring,
            MfaCoveragePercent = Math.Round(mfaCoverage, 1),
            ComplianceStatus = compliance.OverallStatus,
            RecentAudits = audits,
        };
    }
}

/// <summary>
/// SignalR notifications for security events.
/// </summary>
public sealed class SecurityNotificationService : ISecurityNotificationService
{
    private readonly IHubContext<SecurityHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityNotificationService"/> class.
    /// </summary>
    public SecurityNotificationService(IHubContext<SecurityHub> hubContext) =>
        this.hubContext = hubContext;

    /// <inheritdoc />
    public Task NotifySecurityAlertAsync(
        Guid organizationId,
        string alertType,
        string message,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "SecurityAlert", new { alertType, message }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyAuditEventAsync(
        Guid organizationId,
        string eventType,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "AuditEvent", new { eventType }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyPolicyViolationAsync(
        Guid organizationId,
        string message,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PolicyViolation", new { message }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyNewLoginAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "NewLogin", new { userId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyCredentialChangeAsync(
        Guid organizationId,
        string providerName,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "CredentialChange", new { providerName }, cancellationToken);

    private Task SendAsync(Guid organizationId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(SecurityHub.GetOrganizationGroupName(organizationId))
            .SendAsync(method, payload, cancellationToken);
}

/// <summary>
/// No-op security notifications for Testing.
/// </summary>
public sealed class NoOpSecurityNotificationService : ISecurityNotificationService
{
    /// <inheritdoc />
    public Task NotifySecurityAlertAsync(
        Guid organizationId,
        string alertType,
        string message,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyAuditEventAsync(
        Guid organizationId,
        string eventType,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPolicyViolationAsync(
        Guid organizationId,
        string message,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyNewLoginAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyCredentialChangeAsync(
        Guid organizationId,
        string providerName,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
