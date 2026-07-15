using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Aggregates live metrics from multiple sources for dashboards.
/// </summary>
public sealed class MetricsAggregatorService : IMetricsAggregator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    private readonly IApplicationDbContext dbContext;
    private readonly IPodOrchestrator orchestrator;
    private readonly IRequestQueue requestQueue;
    private readonly IDateTimeService dateTimeService;
    private readonly IConnectionMultiplexer? redis;
    private readonly ILogger<MetricsAggregatorService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsAggregatorService"/> class.
    /// </summary>
    public MetricsAggregatorService(
        IApplicationDbContext dbContext,
        IPodOrchestrator orchestrator,
        IRequestQueue requestQueue,
        IDateTimeService dateTimeService,
        ILogger<MetricsAggregatorService> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.orchestrator = orchestrator;
        this.requestQueue = requestQueue;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<LiveMetricsSnapshot> GetLiveMetricsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (redis is not null)
        {
            var cached = await TryGetCachedAsync(organizationId);
            if (cached is not null)
            {
                return cached;
            }
        }

        var snapshot = await BuildLiveMetricsAsync(organizationId, cancellationToken);

        if (redis is not null)
        {
            await CacheAsync(organizationId, snapshot);
        }

        return snapshot;
    }

    private async Task<LiveMetricsSnapshot> BuildLiveMetricsAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var now = dateTimeService.UtcNow;
        var cutoff = now.AddMinutes(-15);
        var oneHourAgo = now.AddHours(-1);

        var orchestratorStatus = await orchestrator.GetStatusAsync(organizationId, cancellationToken);
        var queueLength = await requestQueue.GetLengthAsync(organizationId, cancellationToken);

        var recentMetrics = await dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId && m.RecordedAt >= cutoff)
            .ToListAsync(cancellationToken);

        var gpuUtilization = recentMetrics
            .Where(m => m.GpuUtilizationPercent.HasValue)
            .Select(m => m.GpuUtilizationPercent!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var cpuUtilization = orchestratorStatus.RunningPods > 0
            ? Math.Min(100, (double)orchestratorStatus.HealthyPods / orchestratorStatus.RunningPods * 100)
            : 0;

        var gatewayRequests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt >= oneHourAgo)
            .Include(r => r.Latency)
            .ToListAsync(cancellationToken);

        var completed = gatewayRequests.Where(r => r.Status == GatewayRequestStatus.Completed).ToList();
        var failed = gatewayRequests.Count(r =>
            r.Status == GatewayRequestStatus.Failed || r.Status == GatewayRequestStatus.TimedOut);

        var errorRate = gatewayRequests.Count == 0 ? 0 : (double)failed / gatewayRequests.Count;

        var activeStreams = await dbContext.PodPoolMembers
            .Where(m => m.GpuPod.OrganizationId == organizationId)
            .SumAsync(m => m.ActiveStreams, cancellationToken);

        var stoppedPods = await dbContext.GpuPods.CountAsync(
            p => p.OrganizationId == organizationId && p.Status == PodStatus.Stopped,
            cancellationToken);

        var modelsInstalled = await dbContext.AiModels.CountAsync(
            m => m.OrganizationId == organizationId && m.Status != ModelStatus.Deleted,
            cancellationToken);

        var runningPods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId
                && (p.Status == PodStatus.Running
                    || p.Status == PodStatus.Starting
                    || p.Status == PodStatus.Restarting))
            .ToListAsync(cancellationToken);

        var runningPodIds = runningPods.Select(p => p.Id).ToList();
        var providerIds = runningPods.Select(p => p.ProviderId).Distinct().ToList();

        var installedModelSizes = runningPodIds.Count == 0
            ? 0L
            : await dbContext.AiModels
                .Where(m => runningPodIds.Contains(m.PodId) && m.Status != ModelStatus.Deleted)
                .SumAsync(m => m.Size, cancellationToken);

        var gpus = providerIds.Count == 0
            ? []
            : await dbContext.ProviderGpus
                .Where(g => providerIds.Contains(g.ComputeProviderId))
                .ToListAsync(cancellationToken);

        long? gpuMemoryTotal = null;
        foreach (var pod in runningPods)
        {
            var gpu = gpus.FirstOrDefault(g =>
                g.ComputeProviderId == pod.ProviderId
                && (string.IsNullOrEmpty(pod.GpuId)
                    ? g.GpuType == pod.GpuType
                    : g.GpuId == pod.GpuId || g.GpuType == pod.GpuType));

            if (gpu?.MemoryGb is > 0)
            {
                gpuMemoryTotal = (gpuMemoryTotal ?? 0) + (gpu.MemoryGb.Value * 1024L * 1024L * 1024L);
            }
        }

        var memoryUsedFromMetrics = recentMetrics
            .Where(m => m.MemoryUsedBytes.HasValue)
            .Select(m => m.MemoryUsedBytes!.Value)
            .DefaultIfEmpty(0)
            .Sum();

        var gpuMemoryUsed = memoryUsedFromMetrics > 0
            ? memoryUsedFromMetrics
            : installedModelSizes > 0 ? installedModelSizes : (long?)null;

        return new LiveMetricsSnapshot
        {
            CapturedAt = now,
            GpuUtilizationPercent = gpuUtilization,
            CpuUtilizationPercent = cpuUtilization,
            ActiveStreams = activeStreams,
            QueueSize = queueLength,
            RequestsPerSecond = orchestratorStatus.RequestsPerSecond,
            AverageLatencyMs = orchestratorStatus.AverageLatencyMs,
            ErrorRate = errorRate,
            RunningPods = orchestratorStatus.RunningPods,
            HealthyPods = orchestratorStatus.HealthyPods,
            FailedPods = orchestratorStatus.FailedPods,
            StoppedPods = stoppedPods,
            ModelsInstalled = modelsInstalled,
            GpuMemoryUsedBytes = gpuMemoryUsed,
            GpuMemoryTotalBytes = gpuMemoryTotal,
            InferenceCountLastHour = completed.Count,
            TokensGeneratedLastHour = completed.Count * 500L,
        };
    }

    private async Task<LiveMetricsSnapshot?> TryGetCachedAsync(Guid organizationId)
    {
        try
        {
            var db = redis!.GetDatabase();
            var value = await db.StringGetAsync(ObservabilityRedisKeys.LiveMetrics(organizationId));
            if (!value.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<LiveMetricsSnapshot>((string)value!);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read live metrics cache for organization {OrganizationId}", organizationId);
            return null;
        }
    }

    private async Task CacheAsync(Guid organizationId, LiveMetricsSnapshot snapshot)
    {
        try
        {
            var db = redis!.GetDatabase();
            var json = JsonSerializer.Serialize(snapshot);
            await db.StringSetAsync(
                ObservabilityRedisKeys.LiveMetrics(organizationId),
                json,
                CacheTtl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cache live metrics for organization {OrganizationId}", organizationId);
        }
    }
}
