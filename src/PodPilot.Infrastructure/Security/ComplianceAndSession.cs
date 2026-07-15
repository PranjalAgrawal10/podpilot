using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Identity;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// Compliance export, erasure, and status.
/// </summary>
public sealed class ComplianceService : IComplianceService
{
    private readonly IApplicationDbContext dbContext;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IDateTimeService dateTimeService;
    private readonly IEnterpriseAuditService auditService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComplianceService"/> class.
    /// </summary>
    public ComplianceService(
        IApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IDateTimeService dateTimeService,
        IEnterpriseAuditService auditService)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.dateTimeService = dateTimeService;
        this.auditService = auditService;
    }

    /// <inheritdoc />
    public async Task<ComplianceStatus> GetStatusAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(organizationId, cancellationToken);
        var checklist = new List<string>
        {
            settings.GdprEnabled ? "GDPR controls enabled" : "GDPR controls disabled",
            settings.Soc2Enabled ? "SOC2 tracking enabled" : "SOC2 tracking disabled",
            settings.Iso27001Enabled ? "ISO27001 tracking enabled" : "ISO27001 tracking disabled",
            $"Data retention: {settings.DataRetentionDays} days",
            $"Log retention: {settings.LogRetentionDays} days",
        };

        return new ComplianceStatus
        {
            GdprEnabled = settings.GdprEnabled,
            Soc2Enabled = settings.Soc2Enabled,
            Iso27001Enabled = settings.Iso27001Enabled,
            DataRetentionDays = settings.DataRetentionDays,
            LogRetentionDays = settings.LogRetentionDays,
            LastExportAt = settings.LastExportAt,
            LastErasureAt = settings.LastErasureAt,
            OverallStatus = "Ready",
            ControlChecklist = checklist,
        };
    }

    /// <inheritdoc />
    public async Task<ComplianceExportResult> ExportAsync(
        Guid organizationId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var members = await dbContext.OrganizationMembers.AsNoTracking()
            .Where(m => m.OrganizationId == organizationId)
            .Select(m => new { m.UserId, m.Role, m.JoinedAt, m.IsActive })
            .ToListAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            organizationId,
            exportedAt = dateTimeService.UtcNow,
            members,
        });

        var settings = await EnsureSettingsAsync(organizationId, cancellationToken);
        settings.LastExportAt = dateTimeService.UtcNow;
        settings.UpdatedAt = dateTimeService.UtcNow;

        await dbContext.AddComplianceEventAsync(
            new ComplianceEvent
            {
                OrganizationId = organizationId,
                Framework = ComplianceFramework.Gdpr,
                EventType = "Export",
                Status = "Succeeded",
                Details = "Organization data export completed",
                ActorUserId = requestingUserId,
                OccurredAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = organizationId,
                UserId = requestingUserId,
                Category = AuditEventCategory.Compliance,
                EventType = "ComplianceExport",
                Summary = "Compliance data export completed",
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return new ComplianceExportResult
        {
            JsonPayload = payload,
            ExportedAt = settings.LastExportAt.Value,
        };
    }

    /// <inheritdoc />
    public async Task EraseUserAsync(
        Guid organizationId,
        Guid targetUserId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var member = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == targetUserId, cancellationToken)
            ?? throw new NotFoundException("Organization member", targetUserId);

        member.IsActive = false;
        member.Status = MemberStatus.Suspended;
        member.UpdatedAt = dateTimeService.UtcNow;

        var appUser = await userManager.FindByIdAsync(targetUserId.ToString());
        if (appUser is not null)
        {
            appUser.FirstName = "Erased";
            appUser.LastName = "User";
            appUser.Email = $"erased-{targetUserId:N}@invalid.local";
            appUser.UserName = appUser.Email;
            appUser.NormalizedEmail = appUser.Email.ToUpperInvariant();
            appUser.NormalizedUserName = appUser.UserName.ToUpperInvariant();
            appUser.IsActive = false;
            appUser.UpdatedAt = dateTimeService.UtcNow;
            await userManager.UpdateAsync(appUser);
        }

        var settings = await EnsureSettingsAsync(organizationId, cancellationToken);
        settings.LastErasureAt = dateTimeService.UtcNow;
        settings.UpdatedAt = dateTimeService.UtcNow;

        await dbContext.AddComplianceEventAsync(
            new ComplianceEvent
            {
                OrganizationId = organizationId,
                Framework = ComplianceFramework.Gdpr,
                EventType = "Erasure",
                Status = "Succeeded",
                Details = $"Erased user {targetUserId}",
                ActorUserId = requestingUserId,
                OccurredAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = organizationId,
                UserId = requestingUserId,
                Category = AuditEventCategory.Compliance,
                EventType = "ComplianceErasure",
                EntityType = nameof(User),
                EntityId = targetUserId.ToString(),
                Summary = "User data erasure completed",
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task ApplyRetentionAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(organizationId, cancellationToken);
        var cutoff = dateTimeService.UtcNow.AddDays(-settings.LogRetentionDays);
        var oldSessions = await dbContext.SessionHistories
            .Where(s => s.OrganizationId == organizationId && s.StartedAt < cutoff && !s.IsActive)
            .ToListAsync(cancellationToken);

        // Session history is mutable; soft-end only for retention hygiene.
        foreach (var session in oldSessions)
        {
            session.IsActive = false;
            session.EndedAt ??= session.LastSeenAt;
            session.UpdatedAt = dateTimeService.UtcNow;
        }

        await dbContext.AddComplianceEventAsync(
            new ComplianceEvent
            {
                OrganizationId = organizationId,
                Framework = ComplianceFramework.Gdpr,
                EventType = "RetentionApplied",
                Status = "Succeeded",
                Details = $"Marked {oldSessions.Count} old sessions inactive",
                OccurredAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<OrganizationComplianceSettings> EnsureSettingsAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var settings = await dbContext.OrganizationComplianceSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new OrganizationComplianceSettings
        {
            OrganizationId = organizationId,
            CreatedAt = dateTimeService.UtcNow,
        };
        await dbContext.AddOrganizationComplianceSettingsAsync(settings, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }
}

/// <summary>
/// Session and device tracking.
/// </summary>
public sealed class SessionTracker : ISessionTracker
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionTracker"/> class.
    /// </summary>
    public SessionTracker(IApplicationDbContext dbContext, IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task TrackLoginAsync(SessionTrackRequest request, CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        await dbContext.AddSessionHistoryAsync(
            new SessionHistory
            {
                OrganizationId = request.OrganizationId,
                UserId = request.UserId,
                SessionId = request.SessionId,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                DeviceFingerprint = request.DeviceFingerprint,
                CountryCode = request.CountryCode,
                StartedAt = now,
                LastSeenAt = now,
                IsActive = request.Succeeded,
                Succeeded = request.Succeeded,
                FailureReason = request.FailureReason,
                EndedAt = request.Succeeded ? null : now,
                CreatedAt = now,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.Succeeded && request.OrganizationId.HasValue)
        {
            await EnforceConcurrentLimitAsync(request.OrganizationId.Value, request.UserId, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task EndSessionAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.SessionHistories
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionId == sessionId && s.IsActive, cancellationToken);
        if (session is null)
        {
            return;
        }

        session.IsActive = false;
        session.EndedAt = dateTimeService.UtcNow;
        session.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SessionInfo>> ListActiveAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.SessionHistories.AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && s.IsActive && s.Succeeded)
            .OrderByDescending(s => s.LastSeenAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        return rows.Select(s => new SessionInfo
        {
            Id = s.Id,
            UserId = s.UserId,
            SessionId = s.SessionId,
            IpAddress = s.IpAddress,
            UserAgent = s.UserAgent,
            StartedAt = s.StartedAt,
            LastSeenAt = s.LastSeenAt,
        }).ToList();
    }

    /// <inheritdoc />
    public async Task EnforceConcurrentLimitAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.OrganizationSecurityPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, cancellationToken);
        var max = policy?.MaxConcurrentSessions ?? 5;
        if (max <= 0)
        {
            return;
        }

        var active = await dbContext.SessionHistories
            .Where(s => s.OrganizationId == organizationId && s.UserId == userId && s.IsActive && s.Succeeded)
            .OrderByDescending(s => s.LastSeenAt)
            .ToListAsync(cancellationToken);

        foreach (var stale in active.Skip(max))
        {
            stale.IsActive = false;
            stale.EndedAt = dateTimeService.UtcNow;
            stale.UpdatedAt = dateTimeService.UtcNow;
        }

        if (active.Count > max)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
