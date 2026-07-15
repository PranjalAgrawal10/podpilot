using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Contracts.Auth;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Auth.Commands.Login;

/// <summary>
/// Handles user login.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService identityService;
    private readonly IAuthTokenIssuer authTokenIssuer;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IMfaService mfaService;
    private readonly IMfaChallengeStore mfaChallengeStore;
    private readonly ISessionTracker sessionTracker;
    private readonly IEnterpriseAuditService enterpriseAuditService;
    private readonly ISecurityAlertService securityAlertService;
    private readonly IPolicyEngine policyEngine;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IDateTimeService dateTimeService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    public LoginCommandHandler(
        IIdentityService identityService,
        IAuthTokenIssuer authTokenIssuer,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IMfaService mfaService,
        IMfaChallengeStore mfaChallengeStore,
        ISessionTracker sessionTracker,
        IEnterpriseAuditService enterpriseAuditService,
        ISecurityAlertService securityAlertService,
        IPolicyEngine policyEngine,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IDateTimeService dateTimeService,
        IApplicationDbContext dbContext)
    {
        this.identityService = identityService;
        this.authTokenIssuer = authTokenIssuer;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.mfaService = mfaService;
        this.mfaChallengeStore = mfaChallengeStore;
        this.sessionTracker = sessionTracker;
        this.enterpriseAuditService = enterpriseAuditService;
        this.securityAlertService = securityAlertService;
        this.policyEngine = policyEngine;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dateTimeService = dateTimeService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        var user = await identityService.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            await TrackFailedLoginAsync(existingUser, cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var membership = await organizationAuthorizationService.GetDefaultMembershipAsync(user.Id, cancellationToken);
        if (membership is not null)
        {
            await policyEngine.EnsureIpAllowedAsync(
                membership.OrganizationId,
                httpContextService.IpAddress,
                cancellationToken);
        }

        if (await mfaService.IsEnabledAsync(user.Id, cancellationToken))
        {
            var mfaToken = Guid.NewGuid().ToString("N");
            await mfaChallengeStore.StoreAsync(mfaToken, user.Id, TimeSpan.FromMinutes(5), cancellationToken);

            await enterpriseAuditService.AppendAsync(
                new EnterpriseAuditEntry
                {
                    OrganizationId = membership?.OrganizationId,
                    UserId = user.Id,
                    ActorEmail = user.Email,
                    Category = AuditEventCategory.Authentication,
                    EventType = "MfaChallengeIssued",
                    EntityType = nameof(User),
                    EntityId = user.Id.ToString(),
                    Summary = "Login succeeded; MFA challenge required",
                    IpAddress = httpContextService.IpAddress,
                    CorrelationId = httpContextService.CorrelationId,
                    OccurredAt = dateTimeService.UtcNow,
                },
                cancellationToken);

            return new AuthResponse
            {
                RequiresMfa = true,
                MfaToken = mfaToken,
                User = new UserSummary
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                },
            };
        }

        var sessionId = Guid.NewGuid().ToString("N");
        await sessionTracker.TrackLoginAsync(
            new SessionTrackRequest
            {
                UserId = user.Id,
                OrganizationId = membership?.OrganizationId,
                SessionId = sessionId,
                IpAddress = httpContextService.IpAddress,
                Succeeded = true,
            },
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Login,
            nameof(User),
            user.Id.ToString(),
            "User logged in",
            user.Id,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await enterpriseAuditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = membership?.OrganizationId,
                UserId = user.Id,
                ActorEmail = user.Email,
                Category = AuditEventCategory.Authentication,
                EventType = "LoginSucceeded",
                EntityType = nameof(User),
                EntityId = user.Id.ToString(),
                Summary = "User logged in",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return await authTokenIssuer.IssueTokensAsync(user.Id, organizationId: membership?.OrganizationId, cancellationToken);
    }

    private async Task TrackFailedLoginAsync(User? knownUser, CancellationToken cancellationToken)
    {
        Guid? organizationId = null;
        if (knownUser is not null)
        {
            var membership = await organizationAuthorizationService.GetDefaultMembershipAsync(
                knownUser.Id,
                cancellationToken);
            organizationId = membership?.OrganizationId;

            await sessionTracker.TrackLoginAsync(
                new SessionTrackRequest
                {
                    UserId = knownUser.Id,
                    OrganizationId = organizationId,
                    SessionId = Guid.NewGuid().ToString("N"),
                    IpAddress = httpContextService.IpAddress,
                    Succeeded = false,
                    FailureReason = "InvalidCredentials",
                },
                cancellationToken);
        }

        if (!organizationId.HasValue)
        {
            return;
        }

        var since = dateTimeService.UtcNow.AddHours(-1);
        var failures = await dbContext.SessionHistories
            .CountAsync(
                s => s.OrganizationId == organizationId &&
                     !s.Succeeded &&
                     s.StartedAt >= since,
                cancellationToken);

        var threshold = 5;
        var policy = await dbContext.OrganizationSecurityPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId.Value, cancellationToken);
        if (policy is not null)
        {
            threshold = policy.FailedLoginAlertThreshold;
        }

        if (failures >= threshold)
        {
            await securityAlertService.RaiseAsync(
                new SecurityAlert
                {
                    OrganizationId = organizationId.Value,
                    AlertType = SecurityAlertType.FailedLoginAttempts,
                    Message = $"Failed login threshold exceeded ({failures} in the last hour).",
                    UserId = knownUser?.Id,
                    IpAddress = httpContextService.IpAddress,
                },
                cancellationToken);
        }
    }
}
