using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Collects metrics from pods, gateway, scheduler, and health data.
/// </summary>
public sealed class MetricsCollectorService : IMetricsCollector
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPodOrchestrator orchestrator;
    private readonly IRequestQueue requestQueue;
    private readonly IDateTimeService dateTimeService;
    private readonly IObservabilityNotificationService notificationService;
    private readonly ILogger<MetricsCollectorService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollectorService"/> class.
    /// </summary>
    public MetricsCollectorService(
        IApplicationDbContext dbContext,
        IPodOrchestrator orchestrator,
        IRequestQueue requestQueue,
        IDateTimeService dateTimeService,
        IObservabilityNotificationService notificationService,
        ILogger<MetricsCollectorService> logger)
    {
        this.dbContext = dbContext;
        this.orchestrator = orchestrator;
        this.requestQueue = requestQueue;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<MetricsSnapshotData> CollectAndPersistAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var cutoff = now.AddMinutes(-15);

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

        var cpuUtilization = recentMetrics.Count > 0
            ? Math.Min(100, (double)orchestratorStatus.RunningPods / Math.Max(1, orchestratorStatus.RunningPods + orchestratorStatus.FailedPods) * 100)
            : 0;

        var memoryUsed = recentMetrics
            .Where(m => m.MemoryUsedBytes.HasValue)
            .Select(m => m.MemoryUsedBytes!.Value)
            .Sum();

        var diskUsed = recentMetrics
            .Where(m => m.DiskUsedBytes.HasValue)
            .Select(m => m.DiskUsedBytes!.Value)
            .Sum();

        var oneHourAgo = now.AddHours(-1);
        var gatewayRequests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt >= oneHourAgo)
            .Include(r => r.Latency)
            .ToListAsync(cancellationToken);

        var completedRequests = gatewayRequests
            .Where(r => r.Status == GatewayRequestStatus.Completed)
            .ToList();

        var failedRequests = gatewayRequests
            .Count(r => r.Status == GatewayRequestStatus.Failed || r.Status == GatewayRequestStatus.TimedOut);

        var errorRate = gatewayRequests.Count == 0
            ? 0
            : (double)failedRequests / gatewayRequests.Count;

        var averageLatency = completedRequests
            .Where(r => r.Latency is not null)
            .Select(r => (double)r.Latency!.TotalLatencyMs)
            .DefaultIfEmpty(orchestratorStatus.AverageLatencyMs)
            .Average();

        var activeStreams = await dbContext.PodPoolMembers
            .Where(m => m.GpuPod.OrganizationId == organizationId)
            .SumAsync(m => m.ActiveStreams, cancellationToken);

        var runningPods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId
                && (p.Status == PodStatus.Running
                    || p.Status == PodStatus.Starting
                    || p.Status == PodStatus.Restarting))
            .ToListAsync(cancellationToken);

        var diskTotalBytes = runningPods.Sum(p => (long)(p.ContainerDisk + p.VolumeDisk) * 1024L * 1024L * 1024L);
        var runningPodIds = runningPods.Select(p => p.Id).ToList();
        var providerIds = runningPods.Select(p => p.ProviderId).Distinct().ToList();

        var modelSizes = runningPodIds.Count == 0
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

        var gpuMemoryUsed = memoryUsed > 0
            ? memoryUsed
            : modelSizes > 0 ? modelSizes : (long?)null;

        var resolvedDiskUsed = diskUsed > 0
            ? diskUsed
            : modelSizes > 0 ? modelSizes : (long?)null;

        var data = new MetricsSnapshotData
        {
            OrganizationId = organizationId,
            RecordedAt = now,
            GpuUtilizationPercent = gpuUtilization,
            CpuUtilizationPercent = cpuUtilization,
            GpuMemoryUsedBytes = gpuMemoryUsed,
            GpuMemoryTotalBytes = gpuMemoryTotal,
            MemoryUsedBytes = memoryUsed > 0 ? memoryUsed : null,
            MemoryTotalBytes = null,
            DiskUsedBytes = resolvedDiskUsed,
            DiskTotalBytes = diskTotalBytes > 0 ? diskTotalBytes : null,
            NetworkInBytes = 0,
            NetworkOutBytes = 0,
            TemperatureCelsius = null,
            PowerWatts = null,
            ActiveStreams = activeStreams,
            QueueSize = queueLength,
            InferenceCount = completedRequests.Count,
            TokensGenerated = completedRequests.Count * 500L,
            AverageLatencyMs = averageLatency,
            ErrorRate = errorRate,
        };

        var snapshot = new MetricsSnapshot
        {
            OrganizationId = organizationId,
            RecordedAt = now,
            GpuUtilizationPercent = data.GpuUtilizationPercent,
            GpuMemoryUsedBytes = data.GpuMemoryUsedBytes,
            GpuMemoryTotalBytes = data.GpuMemoryTotalBytes,
            CpuUtilizationPercent = data.CpuUtilizationPercent,
            MemoryUsedBytes = data.MemoryUsedBytes,
            MemoryTotalBytes = data.MemoryTotalBytes,
            DiskUsedBytes = data.DiskUsedBytes,
            DiskTotalBytes = data.DiskTotalBytes,
            NetworkInBytes = data.NetworkInBytes,
            NetworkOutBytes = data.NetworkOutBytes,
            TemperatureCelsius = data.TemperatureCelsius,
            PowerWatts = data.PowerWatts,
            ActiveStreams = data.ActiveStreams,
            QueueSize = data.QueueSize,
            InferenceCount = data.InferenceCount,
            TokensGenerated = data.TokensGenerated,
            AverageLatencyMs = data.AverageLatencyMs,
            ErrorRate = data.ErrorRate,
        };

        await dbContext.AddMetricsSnapshotAsync(snapshot, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyMetricsUpdatedAsync(organizationId, cancellationToken);
        await notificationService.NotifyQueueUpdatedAsync(organizationId, queueLength, cancellationToken);

        logger.LogDebug(
            "Collected metrics for organization {OrganizationId}: GPU {GpuUtil}%, Queue {QueueSize}",
            organizationId,
            data.GpuUtilizationPercent,
            data.QueueSize);

        return data;
    }

    /// <inheritdoc />
    public async Task<UsageStatisticsData> CollectUsageAndPersistAsync(
        Guid organizationId,
        MetricsPeriodFilter period,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var from = period.From ?? GetPeriodStart(now, period.Period);
        var to = period.To ?? now;

        var gatewayRequests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt >= from && r.CreatedAt <= to)
            .Include(r => r.Latency)
            .ToListAsync(cancellationToken);

        var completed = gatewayRequests.Where(r => r.Status == GatewayRequestStatus.Completed).ToList();
        var errors = gatewayRequests.Count(r =>
            r.Status == GatewayRequestStatus.Failed || r.Status == GatewayRequestStatus.TimedOut);

        var totalLatency = completed
            .Where(r => r.Latency is not null)
            .Sum(r => (long)r.Latency!.TotalLatencyMs);

        var runningPods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId && p.Status == PodStatus.Running)
            .ToListAsync(cancellationToken);

        var uptimeSeconds = runningPods.Sum(p =>
        {
            if (p.LastStartedAt is null)
            {
                return 0L;
            }

            var start = p.LastStartedAt.Value < from ? from : p.LastStartedAt.Value;
            return (long)Math.Max(0, (to - start).TotalSeconds);
        });

        var data = new UsageStatisticsData
        {
            OrganizationId = organizationId,
            RecordedAt = now,
            Period = period.Period,
            RequestCount = gatewayRequests.Count,
            TokenCount = completed.Count * 500L,
            InferenceCount = completed.Count,
            TotalLatencyMs = totalLatency,
            ErrorCount = errors,
            UptimeSeconds = uptimeSeconds,
        };

        var statistics = new UsageStatistics
        {
            OrganizationId = organizationId,
            RecordedAt = now,
            Period = period.Period,
            RequestCount = data.RequestCount,
            TokenCount = data.TokenCount,
            InferenceCount = data.InferenceCount,
            TotalLatencyMs = data.TotalLatencyMs,
            ErrorCount = data.ErrorCount,
            UptimeSeconds = data.UptimeSeconds,
        };

        await dbContext.AddUsageStatisticsAsync(statistics, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return data;
    }

    private static DateTime GetPeriodStart(DateTime now, MetricsPeriod period) =>
        period switch
        {
            MetricsPeriod.Hourly => now.AddHours(-1),
            MetricsPeriod.Daily => now.AddDays(-1),
            MetricsPeriod.Weekly => now.AddDays(-7),
            MetricsPeriod.Monthly => now.AddDays(-30),
            _ => now.AddHours(-1),
        };
}
