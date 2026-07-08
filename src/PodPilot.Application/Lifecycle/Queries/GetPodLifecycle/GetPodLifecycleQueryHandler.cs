using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Lifecycle.Queries.GetPodLifecycle;

/// <summary>
/// Handles get pod lifecycle queries.
/// </summary>
public sealed class GetPodLifecycleQueryHandler : IRequestHandler<GetPodLifecycleQuery, PodLifecycleSummaryResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodLifecycleService lifecycleService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPodLifecycleQueryHandler"/> class.
    /// </summary>
    public GetPodLifecycleQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodLifecycleService lifecycleService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.lifecycleService = lifecycleService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<PodLifecycleSummaryResponse> Handle(
        GetPodLifecycleQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodRead,
            cancellationToken);

        var pod = await PodAccess.GetPodAsync(dbContext, request.PodId, organizationId, cancellationToken);
        var policy = await lifecycleService.GetOrCreateIdlePolicyAsync(request.PodId, cancellationToken);
        var now = dateTimeService.UtcNow;
        var idleStatus = lifecycleService.EvaluateIdleStatus(pod, policy, now);

        var runningReference = pod.LastStartedAt ?? pod.CreatedAt;
        var runningTimeMinutes = pod.Status == PodStatus.Running
            ? Math.Max(0, (now - runningReference).TotalMinutes)
            : 0;

        return new PodLifecycleSummaryResponse
        {
            PodId = pod.Id,
            Status = pod.Status.ToString(),
            RunningTimeMinutes = runningTimeMinutes,
            IdleTimeMinutes = idleStatus.IdleMinutes,
            LastActivityAt = pod.LastActivityAt ?? pod.LastStartedAt ?? pod.CreatedAt,
            NextShutdownAt = idleStatus.NextShutdownAt,
            AutoWakeEnabled = policy.AutoWakeEnabled,
            AutoShutdownEnabled = policy.AutoShutdownEnabled,
            IsIdle = idleStatus.IsIdle,
            Policy = LifecycleMapper.ToResponse(policy),
        };
    }
}
