using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Scheduler;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Orchestrator;

/// <summary>
/// Calculates capacity metrics and records snapshots.
/// </summary>
public sealed class CapacityPlannerService : ICapacityPlanner
{
    private const double EstimatedRequestsPerSecondPerPod = 2.0;

    private readonly IApplicationDbContext dbContext;
    private readonly IPodPoolManager podPoolManager;
    private readonly IRequestQueue requestQueue;
    private readonly IDateTimeService dateTimeService;
    private readonly IConnectionMultiplexer? redis;
    private readonly ILogger<CapacityPlannerService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityPlannerService"/> class.
    /// </summary>
    public CapacityPlannerService(
        IApplicationDbContext dbContext,
        IPodPoolManager podPoolManager,
        IRequestQueue requestQueue,
        IDateTimeService dateTimeService,
        ILogger<CapacityPlannerService> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.podPoolManager = podPoolManager;
        this.requestQueue = requestQueue;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<CapacityPlan> CalculateAsync(
        Guid organizationId,
        Guid? poolId = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PodPool> pools;

        if (poolId.HasValue)
        {
            var pool = await dbContext.PodPools
                .FirstOrDefaultAsync(p => p.Id == poolId.Value && p.OrganizationId == organizationId, cancellationToken);

            pools = pool is null ? [] : [pool];
        }
        else
        {
            pools = await dbContext.PodPools
                .Where(p => p.OrganizationId == organizationId && p.IsActive)
                .ToListAsync(cancellationToken);
        }

        var queueLength = await requestQueue.GetLengthAsync(organizationId, cancellationToken);
        var totalPods = 0;
        var healthyPods = 0;
        var busyPods = 0;
        var concurrentStreams = 0;
        var totalLoad = 0;
        var latencySamples = new List<double>();
        var gpuUtilSamples = new List<double>();

        foreach (var pool in pools)
        {
            var members = await dbContext.PodPoolMembers
                .Where(m => m.PodPoolId == pool.Id)
                .Include(m => m.GpuPod)
                .ToListAsync(cancellationToken);

            totalPods += members.Count;

            foreach (var member in members)
            {
                if (member.GpuPod.Status == PodStatus.Deleted)
                {
                    continue;
                }

                if (member.State == OrchestrationPodState.Healthy)
                {
                    healthyPods++;
                }
                else if (member.State == OrchestrationPodState.Busy)
                {
                    busyPods++;
                    healthyPods++;
                }

                concurrentStreams += member.ActiveStreams;
                totalLoad += await GetPodLoadAsync(organizationId, member.GpuPodId, member, cancellationToken);
            }

            var contexts = await podPoolManager.GetHealthyMembersAsync(pool.Id, cancellationToken);
            foreach (var context in contexts)
            {
                if (context.AverageLatencyMs > 0)
                {
                    latencySamples.Add(context.AverageLatencyMs);
                }
            }
        }

        var cutoff = dateTimeService.UtcNow.AddMinutes(-15);
        var recentMetrics = await dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId && m.RecordedAt >= cutoff)
            .Where(m => !poolId.HasValue || dbContext.PodPoolMembers.Any(pm => pm.PodPoolId == poolId && pm.GpuPodId == m.GpuPodId))
            .Select(m => m.GpuUtilizationPercent)
            .Where(v => v.HasValue)
            .ToListAsync(cancellationToken);

        gpuUtilSamples.AddRange(recentMetrics.Select(v => v!.Value));

        var routablePods = Math.Max(1, healthyPods);
        var maxCapacity = routablePods * ApplicationConstants.SchedulerMaxConcurrentPerPod;
        var currentCapacity = maxCapacity == 0 ? 1.0 : Math.Min(1.0, (double)totalLoad / maxCapacity);
        var projectedCapacity = Math.Min(1.0, currentCapacity + (queueLength / (double)Math.Max(1, maxCapacity)));
        var remainingCapacity = Math.Max(0, 1.0 - currentCapacity);
        var averageLatencyMs = latencySamples.Count > 0 ? latencySamples.Average() : 0;
        var averageWaitTimeMs = queueLength > 0 ? averageLatencyMs * queueLength : 0;
        var gpuUtilization = gpuUtilSamples.Count > 0 ? gpuUtilSamples.Average() : currentCapacity * 100;

        var suggestedScale = 0;
        if (queueLength > 0 || currentCapacity >= 0.8)
        {
            suggestedScale = 1;
        }
        else if (currentCapacity <= 0.3 && totalPods > 1)
        {
            suggestedScale = -1;
        }

        return new CapacityPlan
        {
            OrganizationId = organizationId,
            PoolId = poolId,
            CurrentCapacity = currentCapacity,
            ProjectedCapacity = projectedCapacity,
            RemainingCapacity = remainingCapacity,
            MaximumThroughput = routablePods * EstimatedRequestsPerSecondPerPod,
            SuggestedScale = suggestedScale,
            TotalPods = totalPods,
            HealthyPods = healthyPods,
            BusyPods = busyPods,
            QueueLength = queueLength,
            AverageWaitTimeMs = averageWaitTimeMs,
            AverageLatencyMs = averageLatencyMs,
            GpuUtilizationPercent = gpuUtilization,
            ConcurrentStreams = concurrentStreams,
        };
    }

    /// <inheritdoc />
    public async Task RecordSnapshotAsync(
        Guid organizationId,
        Guid? poolId = null,
        CancellationToken cancellationToken = default)
    {
        var plan = await CalculateAsync(organizationId, poolId, cancellationToken);
        var snapshot = new CapacitySnapshot
        {
            OrganizationId = organizationId,
            PodPoolId = poolId,
            RecordedAt = dateTimeService.UtcNow,
            TotalPods = plan.TotalPods,
            HealthyPods = plan.HealthyPods,
            BusyPods = plan.BusyPods,
            QueueLength = plan.QueueLength,
            AverageWaitTimeMs = plan.AverageWaitTimeMs,
            AverageLatencyMs = plan.AverageLatencyMs,
            GpuUtilizationPercent = plan.GpuUtilizationPercent,
            ConcurrentStreams = plan.ConcurrentStreams,
            CurrentCapacity = plan.CurrentCapacity,
            ProjectedCapacity = plan.ProjectedCapacity,
            RemainingCapacity = plan.RemainingCapacity,
            MaximumThroughput = plan.MaximumThroughput,
            SuggestedScale = plan.SuggestedScale,
        };

        await dbContext.AddCapacitySnapshotAsync(snapshot, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogDebug(
            "Recorded capacity snapshot for organization {OrganizationId}, pool {PoolId}",
            organizationId,
            poolId);
    }

    private async Task<int> GetPodLoadAsync(
        Guid organizationId,
        Guid podId,
        PodPoolMember member,
        CancellationToken cancellationToken)
    {
        if (redis is not null)
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync(SchedulerRedisKeys.PodLoad(organizationId, podId));
            if (value.HasValue && int.TryParse(value.ToString(), out var load))
            {
                return load;
            }
        }

        var activeRequests = await dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId
                && r.GpuPodId == podId
                && (r.Status == GatewayRequestStatus.Forwarding
                    || r.Status == GatewayRequestStatus.Streaming
                    || r.Status == GatewayRequestStatus.Waking
                    || r.Status == GatewayRequestStatus.WaitingHealthy),
            cancellationToken);

        return Math.Max(activeRequests, member.ActiveStreams);
    }
}
