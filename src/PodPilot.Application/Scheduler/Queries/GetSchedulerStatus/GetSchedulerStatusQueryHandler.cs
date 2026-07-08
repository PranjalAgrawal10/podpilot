using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Scheduler;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Scheduler.Queries.GetSchedulerStatus;

/// <summary>
/// Handles <see cref="GetSchedulerStatusQuery"/>.
/// </summary>
public sealed class GetSchedulerStatusQueryHandler : IRequestHandler<GetSchedulerStatusQuery, SchedulerStatusResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IRequestQueue requestQueue;
    private readonly IRequestTracker requestTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerStatusQueryHandler"/> class.
    /// </summary>
    public GetSchedulerStatusQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IRequestQueue requestQueue,
        IRequestTracker requestTracker)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.requestQueue = requestQueue;
        this.requestTracker = requestTracker;
    }

    /// <inheritdoc />
    public async Task<SchedulerStatusResponse> Handle(GetSchedulerStatusQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);
        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var orgRequests = dbContext.GatewayRequests.Where(r => r.OrganizationId == organizationId);
        var queued = await requestQueue.GetLengthAsync(organizationId, cancellationToken);
        var running = await orgRequests.CountAsync(
            r => r.Status == GatewayRequestStatus.Forwarding
                || r.Status == GatewayRequestStatus.Streaming
                || r.Status == GatewayRequestStatus.Waking
                || r.Status == GatewayRequestStatus.WaitingHealthy,
            cancellationToken);
        var retries = await orgRequests.CountAsync(
            r => r.RetryCount > 0 && r.CreatedAt >= oneHourAgo,
            cancellationToken);
        var activePods = await dbContext.GpuPods.CountAsync(
            p => p.OrganizationId == organizationId && p.Status == PodStatus.Running,
            cancellationToken);
        var maxCapacity = Math.Max(activePods * ApplicationConstants.SchedulerMaxConcurrentPerPod, 1);
        var utilization = Math.Round((double)running / maxCapacity * 100, 1);

        return new SchedulerStatusResponse
        {
            IsHealthy = true,
            ActiveTrackedRequests = requestTracker.ActiveCount,
            TotalQueuedRequests = queued,
            TotalRunningRequests = running,
            RetriesLastHour = retries,
            PodUtilizationPercent = utilization,
        };
    }
}
