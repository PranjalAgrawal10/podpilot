using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Lifecycle.Commands.UpdatePodIdlePolicy;

/// <summary>
/// Handles update pod idle policy commands.
/// </summary>
public sealed class UpdatePodIdlePolicyCommandHandler : IRequestHandler<UpdatePodIdlePolicyCommand, PodIdlePolicyResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodLifecycleService lifecycleService;
    private readonly IPodNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<UpdatePodIdlePolicyCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePodIdlePolicyCommandHandler"/> class.
    /// </summary>
    public UpdatePodIdlePolicyCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodLifecycleService lifecycleService,
        IPodNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<UpdatePodIdlePolicyCommandHandler> logger)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.lifecycleService = lifecycleService;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<PodIdlePolicyResponse> Handle(
        UpdatePodIdlePolicyCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodUpdate,
            cancellationToken);

        await PodAccess.GetPodAsync(dbContext, request.PodId, organizationId, cancellationToken);

        var policy = await lifecycleService.GetOrCreateIdlePolicyAsync(request.PodId, cancellationToken);
        policy.IdleTimeoutMinutes = request.IdleTimeoutMinutes;
        policy.GracePeriodMinutes = request.GracePeriodMinutes;
        policy.AutoShutdownEnabled = request.AutoShutdownEnabled;
        policy.AutoWakeEnabled = request.AutoWakeEnabled;
        policy.MinimumRunningTimeMinutes = request.MinimumRunningTimeMinutes;
        policy.IdleDetectedAt = null;
        policy.UpdatedAt = dateTimeService.UtcNow;
        policy.UpdatedBy = userId.ToString();

        var now = dateTimeService.UtcNow;
        await dbContext.AddPodLifecycleEventAsync(
            new PodLifecycleEvent
            {
                PodId = request.PodId,
                EventType = PodLifecycleEventType.PolicyUpdated,
                Timestamp = now,
                Source = "api",
                UserId = userId,
                Message = "Idle policy updated.",
            },
            cancellationToken);

        await lifecycleService.RecordActivityAsync(
            request.PodId,
            PodActivityType.PolicyChange,
            "api",
            userId,
            cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Policy updated for pod {PodId} by user {UserId}", request.PodId, userId);

        await notificationService.NotifyLifecycleEventAsync(
            organizationId,
            request.PodId,
            "PolicyUpdated",
            LifecycleMapper.ToResponse(policy),
            cancellationToken);

        return LifecycleMapper.ToResponse(policy);
    }
}
