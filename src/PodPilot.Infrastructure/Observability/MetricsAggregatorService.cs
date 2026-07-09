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
