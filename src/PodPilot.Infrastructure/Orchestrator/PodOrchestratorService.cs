using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

using PodPilot.Infrastructure.Scheduler;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Orchestrator;

/// <summary>
/// Coordinates multi-pod orchestration, failover, and routing.
/// </summary>
public sealed class PodOrchestratorService : IPodOrchestrator
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPodPoolManager podPoolManager;
    private readonly ILoadBalancer loadBalancer;
    private readonly IRequestQueue requestQueue;
    private readonly IConnectionMultiplexer? redis;
    private readonly IPodLifecycleService lifecycleService;
    private readonly IOrchestratorNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<PodOrchestratorService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodOrchestratorService"/> class.
    /// </summary>
    public PodOrchestratorService(
        IApplicationDbContext dbContext,
        IPodPoolManager podPoolManager,
        ILoadBalancer loadBalancer,
        IRequestQueue requestQueue,
        IPodLifecycleService lifecycleService,
        IOrchestratorNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<PodOrchestratorService> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.podPoolManager = podPoolManager;
        this.loadBalancer = loadBalancer;
        this.requestQueue = requestQueue;
        this.lifecycleService = lifecycleService;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<OrchestratorRouteResult?> ResolvePodAsync(
        OrchestratorRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var pool = await podPoolManager.ResolvePoolAsync(
            request.OrganizationId,
            request.ModelName,
            cancellationToken);

        if (pool is null)
        {
            return null;
        }

        var members = await podPoolManager.GetHealthyMembersAsync(pool.Id, cancellationToken);
        if (members.Count == 0)
        {
            return null;
        }

        if (request.PreferredPodId.HasValue)
        {
            var preferred = members.FirstOrDefault(m => m.PodId == request.PreferredPodId.Value);
            if (preferred is not null)
            {
                return new OrchestratorRouteResult
                {
                    Pod = preferred.Pod,
                    BaseUrl = preferred.BaseUrl,
                    Model = request.ModelName,
                    PoolId = pool.Id,
                    CurrentLoad = preferred.CurrentLoad,
                };
            }
        }

        var selection = await loadBalancer.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = request.OrganizationId,
                PoolId = pool.Id,
                ModelName = request.ModelName,
                SessionKey = request.SessionKey,
                Members = members,
            },
            cancellationToken);

        if (selection is null)
        {
            return null;
        }

        var selectedMember = members.First(m => m.PodId == selection.PodId);
        return new OrchestratorRouteResult
        {
            Pod = selectedMember.Pod,
            BaseUrl = selection.BaseUrl,
            Model = request.ModelName,
            PoolId = pool.Id,
            CurrentLoad = selection.CurrentLoad,
        };
    }

    /// <inheritdoc />
    public async Task<FailoverResult> HandleFailoverAsync(
        Guid organizationId,
        Guid failedPodId,
        Guid? requestId,
        CancellationToken cancellationToken = default)
    {
        var member = await dbContext.PodPoolMembers
            .Include(m => m.PodPool)
            .FirstOrDefaultAsync(m => m.GpuPodId == failedPodId, cancellationToken);

        if (member is not null)
        {
            await podPoolManager.UpdateMemberStateAsync(
                member.PodPoolId,
                failedPodId,
                OrchestrationPodState.Failed,
                cancellationToken);
        }

        await notificationService.NotifyPodFailedAsync(
            organizationId,
            failedPodId,
            "Pod failed during request processing.",
            cancellationToken);

        var poolId = member?.PodPoolId;
        Guid? replacementPodId = null;

        if (poolId.HasValue)
        {
            var members = await podPoolManager.GetHealthyMembersAsync(poolId.Value, cancellationToken);
            var replacement = members.FirstOrDefault(m => m.PodId != failedPodId);
            replacementPodId = replacement?.PodId;

            if (replacementPodId.HasValue)
            {
                var wakeResult = await lifecycleService.WakePodAsync(
                    replacementPodId.Value,
                    organizationId,
                    "orchestrator-failover",
                    processImmediately: true,
                    cancellationToken: cancellationToken);

                if (!wakeResult.Success)
                {
                    logger.LogWarning(
                        "Failed to wake replacement pod {PodId} during failover: {Error}",
                        replacementPodId,
                        wakeResult.ErrorMessage);
                }
            }
        }

        var reassignedCount = 0;
        var queuedRequests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId
                && r.GpuPodId == failedPodId
                && r.Status == GatewayRequestStatus.Queued)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (replacementPodId.HasValue)
        {
            foreach (var queuedRequestId in queuedRequests)
            {
                if (await ReassignRequestAsync(
                    queuedRequestId,
                    replacementPodId.Value,
                    organizationId,
                    failedPodId,
                    cancellationToken))
                {
                    reassignedCount++;
                }
            }
        }

        if (requestId.HasValue && replacementPodId.HasValue && !queuedRequests.Contains(requestId.Value))
        {
            if (await ReassignRequestAsync(
                requestId.Value,
                replacementPodId.Value,
                organizationId,
                failedPodId,
                cancellationToken))
            {
                reassignedCount++;
            }
        }

        await notificationService.NotifyFailoverTriggeredAsync(
            organizationId,
            failedPodId,
            replacementPodId,
            cancellationToken);

        var success = replacementPodId.HasValue;
        return new FailoverResult
        {
            Success = success,
            ReplacementPodId = replacementPodId,
            ReassignedRequestCount = reassignedCount,
            ErrorMessage = success ? null : "No replacement pod was available.",
        };
    }

    /// <inheritdoc />
    public async Task<OrchestratorStatus> GetStatusAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var pools = await dbContext.PodPools
            .Where(p => p.OrganizationId == organizationId && p.IsActive)
            .Include(p => p.Members)
            .ToListAsync(cancellationToken);

        var poolCount = pools.Count;
        var runningPods = 0;
        var healthyPods = 0;
        var drainingPods = 0;
        var failedPods = 0;

        foreach (var pool in pools)
        {
            foreach (var member in pool.Members)
            {
                switch (member.State)
                {
                    case OrchestrationPodState.Healthy:
                    case OrchestrationPodState.Busy:
                        healthyPods++;
                        runningPods++;
                        break;
                    case OrchestrationPodState.Draining:
                        drainingPods++;
                        runningPods++;
                        break;
                    case OrchestrationPodState.Failed:
                        failedPods++;
                        break;
                    case OrchestrationPodState.Starting:
                    case OrchestrationPodState.Warming:
                    case OrchestrationPodState.Provisioning:
                        runningPods++;
                        break;
                }
            }
        }

        var queueLength = await requestQueue.GetLengthAsync(organizationId, cancellationToken);
        var cutoff = dateTimeService.UtcNow.AddMinutes(-15);

        var averageLatencyMs = await dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId && m.RecordedAt >= cutoff)
            .Select(m => (double?)m.LatencyMs)
            .AverageAsync(cancellationToken) ?? 0;

        var recentRequestCount = await dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId && r.CreatedAt >= cutoff,
            cancellationToken);

        var requestsPerSecond = recentRequestCount / Math.Max(1.0, (dateTimeService.UtcNow - cutoff).TotalSeconds);

        return new OrchestratorStatus
        {
            PoolCount = poolCount,
            RunningPods = runningPods,
            HealthyPods = healthyPods,
            DrainingPods = drainingPods,
            FailedPods = failedPods,
            QueueLength = queueLength,
            AverageLatencyMs = averageLatencyMs,
            RequestsPerSecond = requestsPerSecond,
        };
    }

    private async Task<bool> ReassignRequestAsync(
        Guid requestId,
        Guid newPodId,
        Guid organizationId,
        Guid oldPodId,
        CancellationToken cancellationToken)
    {
        var request = await dbContext.GatewayRequests
            .Where(r => r.Id == requestId && r.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return false;
        }

        request.GpuPodId = newPodId;
        await ReleasePodAssignmentAsync(organizationId, oldPodId, requestId, cancellationToken);

        await dbContext.AddSchedulerEventAsync(
            new SchedulerEvent
            {
                GatewayRequestId = request.Id,
                OrganizationId = organizationId,
                EventType = SchedulerEventType.Reassigned,
                Message = $"Reassigned from pod {oldPodId} to {newPodId} during failover.",
                Timestamp = dateTimeService.UtcNow,
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ReleasePodAssignmentAsync(
        Guid organizationId,
        Guid podId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        if (redis is null)
        {
            return;
        }

        var db = redis.GetDatabase();
        await db.StringDecrementAsync(SchedulerRedisKeys.PodLoad(organizationId, podId));
        await db.KeyDeleteAsync(SchedulerRedisKeys.Processing(requestId));
    }
}
