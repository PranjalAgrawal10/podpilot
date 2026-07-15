using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Monitors system health and manages alert detection.
/// </summary>
public sealed class MonitoringService : IMonitoringService
{
    private const int HighGpuThreshold = 90;
    private const int HighQueueThreshold = 20;
    private const int HighLatencyThresholdMs = 5000;
    private const int GatewayErrorThreshold = 10;
    private const double DiskFullThresholdPercent = 90;
    private const double MemoryPressureThresholdPercent = 90;
    private const long BytesPerGigabyte = 1024L * 1024L * 1024L;

    private readonly IApplicationDbContext dbContext;
    private readonly IPodOrchestrator orchestrator;
    private readonly IRequestQueue requestQueue;
    private readonly IConnectionMultiplexer? redis;
    private readonly IDateTimeService dateTimeService;
    private readonly IObservabilityNotificationService notificationService;
    private readonly ILogger<MonitoringService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitoringService"/> class.
    /// </summary>
    public MonitoringService(
        IApplicationDbContext dbContext,
        IPodOrchestrator orchestrator,
        IRequestQueue requestQueue,
        IDateTimeService dateTimeService,
        IObservabilityNotificationService notificationService,
        ILogger<MonitoringService> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.orchestrator = orchestrator;
        this.requestQueue = requestQueue;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<SystemHealthOverview> RunHealthChecksAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var components = new List<ComponentHealthStatus>();

        var podOverview = await GetPodHealthOverviewAsync(organizationId, cancellationToken);
        var providerOverview = await GetProviderHealthOverviewAsync(organizationId, cancellationToken);
        var orchestratorStatus = await orchestrator.GetStatusAsync(organizationId, cancellationToken);
        var queueLength = await requestQueue.GetLengthAsync(organizationId, cancellationToken);

        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.System,
            Status = ResolveOverallStatus(podOverview, providerOverview),
            Message = $"Running pods: {orchestratorStatus.RunningPods}, Queue: {queueLength}",
        });

        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.Pod,
            Status = podOverview.UnhealthyPods > 0
                ? ObservabilityHealthStatus.Unhealthy
                : podOverview.DegradedPods > 0
                    ? ObservabilityHealthStatus.Degraded
                    : ObservabilityHealthStatus.Healthy,
            Message = $"{podOverview.HealthyPods}/{podOverview.TotalPods} pods healthy",
        });

        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.Provider,
            Status = providerOverview.UnhealthyProviders > 0
                ? ObservabilityHealthStatus.Unhealthy
                : ObservabilityHealthStatus.Healthy,
            Message = $"{providerOverview.HealthyProviders}/{providerOverview.TotalProviders} providers healthy",
        });

        var oneHourAgo = now.AddHours(-1);
        var recentErrors = await dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId
                && r.Status == GatewayRequestStatus.Failed
                && r.StartedAt >= oneHourAgo,
            cancellationToken);

        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.Gateway,
            Status = recentErrors >= GatewayErrorThreshold
                ? ObservabilityHealthStatus.Degraded
                : ObservabilityHealthStatus.Healthy,
            Message = $"{recentErrors} gateway errors in the last hour",
        });

        var ollamaUnhealthy = podOverview.Pods.Count(p => !p.OllamaHealthy);
        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.Ollama,
            Status = ollamaUnhealthy > 0
                ? ObservabilityHealthStatus.Unhealthy
                : ObservabilityHealthStatus.Healthy,
            Message = ollamaUnhealthy > 0
                ? $"{ollamaUnhealthy} pods with unhealthy Ollama"
                : "All Ollama endpoints healthy",
        });

        var databaseHealthy = await CheckDatabaseHealthAsync(cancellationToken);
        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.Database,
            Status = databaseHealthy
                ? ObservabilityHealthStatus.Healthy
                : ObservabilityHealthStatus.Unhealthy,
            Message = databaseHealthy ? "Database connection OK" : "Database connection failed",
        });

        var redisHealthy = redis is null || redis.IsConnected;
        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.Redis,
            Status = redisHealthy
                ? ObservabilityHealthStatus.Healthy
                : ObservabilityHealthStatus.Degraded,
            Message = redis is null ? "Redis not configured" : redisHealthy ? "Redis connected" : "Redis disconnected",
        });

        components.Add(new ComponentHealthStatus
        {
            Component = HealthComponent.SignalR,
            Status = ObservabilityHealthStatus.Healthy,
            Message = "SignalR hub available",
        });

        foreach (var component in components)
        {
            await dbContext.AddSystemHealthHistoryAsync(
                new SystemHealthHistory
                {
                    OrganizationId = organizationId,
                    RecordedAt = now,
                    Component = component.Component,
                    Status = component.Status,
                    Message = component.Message,
                    RelatedEntityId = component.RelatedEntityId,
                },
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new SystemHealthOverview
        {
            CheckedAt = now,
            OverallStatus = ResolveOverallStatus(podOverview, providerOverview),
            Components = components,
        };
    }

    /// <inheritdoc />
    public async Task<PodHealthOverview> GetPodHealthOverviewAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var pods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId && p.Status != PodStatus.Deleted && p.Status != PodStatus.Deleting)
            .ToListAsync(cancellationToken);

        var cutoff = now.AddHours(-1);
        var recentMetrics = await dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId && m.RecordedAt >= cutoff)
            .ToListAsync(cancellationToken);

        var latestMetrics = recentMetrics
            .GroupBy(m => m.GpuPodId)
            .Select(g => g.OrderByDescending(m => m.RecordedAt).First())
            .ToList();

        var entries = pods.Select(pod =>
        {
            var metric = latestMetrics.FirstOrDefault(m => m.GpuPodId == pod.Id);
            var status = ResolvePodStatus(pod, metric);

            return new PodHealthEntry
            {
                PodId = pod.Id,
                PodName = pod.Name,
                Status = status,
                GpuHealthy = metric?.GpuHealthy ?? pod.Status == PodStatus.Running,
                OllamaHealthy = metric?.OllamaHealthy ?? false,
                ModelsHealthy = metric?.ModelsHealthy ?? false,
                LatencyMs = metric?.LatencyMs ?? 0,
                GpuUtilizationPercent = metric?.GpuUtilizationPercent,
                ErrorMessage = metric?.ErrorMessage,
                LastCheckedAt = metric?.RecordedAt,
            };
        }).ToList();

        return new PodHealthOverview
        {
            CheckedAt = now,
            TotalPods = entries.Count,
            HealthyPods = entries.Count(e => e.Status == ObservabilityHealthStatus.Healthy),
            DegradedPods = entries.Count(e => e.Status == ObservabilityHealthStatus.Degraded),
            UnhealthyPods = entries.Count(e => e.Status == ObservabilityHealthStatus.Unhealthy),
            Pods = entries,
        };
    }

    /// <inheritdoc />
    public async Task<ProviderHealthOverview> GetProviderHealthOverviewAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var providers = await dbContext.ComputeProviders
            .Where(p => p.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var providerIds = providers.Select(p => p.Id).ToList();
        var healthSnapshots = providerIds.Count == 0
            ? []
            : await dbContext.ProviderHealthSnapshots
                .Where(h => providerIds.Contains(h.ComputeProviderId))
                .ToListAsync(cancellationToken);

        var entries = providers.Select(provider =>
        {
            var health = healthSnapshots.FirstOrDefault(h => h.ComputeProviderId == provider.Id);
            var status = health?.Status switch
            {
                ProviderConnectionStatus.Connected => ObservabilityHealthStatus.Healthy,
                ProviderConnectionStatus.Degraded => ObservabilityHealthStatus.Degraded,
                ProviderConnectionStatus.Disconnected => ObservabilityHealthStatus.Unhealthy,
                _ => ObservabilityHealthStatus.Degraded,
            };

            return new ProviderHealthEntry
            {
                ProviderId = provider.Id,
                ProviderName = provider.Name,
                Status = status,
                ResponseTimeMs = health?.ResponseTimeMs,
                ErrorMessage = health?.ErrorMessage,
                LastCheckedAt = health?.LastCheckedAt,
            };
        }).ToList();

        return new ProviderHealthOverview
        {
            CheckedAt = now,
            TotalProviders = entries.Count,
            HealthyProviders = entries.Count(e => e.Status == ObservabilityHealthStatus.Healthy),
            UnhealthyProviders = entries.Count(e => e.Status == ObservabilityHealthStatus.Unhealthy),
            Providers = entries,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AlertSummary>> EvaluateAlertsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var raisedAlerts = new List<AlertSummary>();

        var podOverview = await GetPodHealthOverviewAsync(organizationId, cancellationToken);
        var orchestratorStatus = await orchestrator.GetStatusAsync(organizationId, cancellationToken);
        var oneHourAgo = now.AddHours(-1);

        foreach (var pod in podOverview.Pods)
        {
            if (pod.GpuUtilizationPercent >= HighGpuThreshold)
            {
                var alert = await RaiseOrUpdateAlertAsync(
                    organizationId,
                    AlertType.HighGpuUsage,
                    AlertSeverity.Warning,
                    $"High GPU usage on {pod.PodName}",
                    $"GPU utilization is {pod.GpuUtilizationPercent:F1}%",
                    gpuPodId: pod.PodId,
                    cancellationToken: cancellationToken);
                raisedAlerts.Add(alert);
            }

            if (pod.Status == ObservabilityHealthStatus.Unhealthy)
            {
                var alert = await RaiseOrUpdateAlertAsync(
                    organizationId,
                    AlertType.PodFailure,
                    AlertSeverity.Critical,
                    $"Pod failure: {pod.PodName}",
                    pod.ErrorMessage ?? "Pod health checks failed",
                    gpuPodId: pod.PodId,
                    cancellationToken: cancellationToken);
                raisedAlerts.Add(alert);

                await notificationService.NotifyPodHealthChangedAsync(
                    organizationId,
                    pod.PodId,
                    pod.Status.ToString(),
                    cancellationToken);
            }

            if (!pod.ModelsHealthy && pod.OllamaHealthy)
            {
                var alert = await RaiseOrUpdateAlertAsync(
                    organizationId,
                    AlertType.ModelFailure,
                    AlertSeverity.Warning,
                    $"Model failure on {pod.PodName}",
                    "No healthy models detected",
                    gpuPodId: pod.PodId,
                    cancellationToken: cancellationToken);
                raisedAlerts.Add(alert);
            }
        }

        if (orchestratorStatus.QueueLength >= HighQueueThreshold)
        {
            var alert = await RaiseOrUpdateAlertAsync(
                organizationId,
                AlertType.HighQueueLength,
                AlertSeverity.Warning,
                "High queue length",
                $"Queue has {orchestratorStatus.QueueLength} pending requests",
                cancellationToken: cancellationToken);
            raisedAlerts.Add(alert);
        }

        if (orchestratorStatus.AverageLatencyMs >= HighLatencyThresholdMs)
        {
            var alert = await RaiseOrUpdateAlertAsync(
                organizationId,
                AlertType.HighLatency,
                AlertSeverity.Warning,
                "High latency detected",
                $"Average latency is {orchestratorStatus.AverageLatencyMs:F0}ms",
                cancellationToken: cancellationToken);
            raisedAlerts.Add(alert);
        }

        var providerOverview = await GetProviderHealthOverviewAsync(organizationId, cancellationToken);
        foreach (var provider in providerOverview.Providers.Where(p => p.Status == ObservabilityHealthStatus.Unhealthy))
        {
            var alert = await RaiseOrUpdateAlertAsync(
                organizationId,
                AlertType.ProviderFailure,
                AlertSeverity.Critical,
                $"Provider failure: {provider.ProviderName}",
                provider.ErrorMessage ?? "Provider is disconnected",
                providerId: provider.ProviderId,
                cancellationToken: cancellationToken);
            raisedAlerts.Add(alert);

            await notificationService.NotifyProviderHealthChangedAsync(
                organizationId,
                provider.ProviderId,
                provider.Status.ToString(),
                cancellationToken);
        }

        var gatewayErrors = await dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId
                && r.Status == GatewayRequestStatus.Failed
                && r.StartedAt >= oneHourAgo,
            cancellationToken);

        if (gatewayErrors >= GatewayErrorThreshold)
        {
            var alert = await RaiseOrUpdateAlertAsync(
                organizationId,
                AlertType.RepeatedGatewayErrors,
                AlertSeverity.Critical,
                "Repeated gateway errors",
                $"{gatewayErrors} gateway errors in the last hour",
                cancellationToken: cancellationToken);
            raisedAlerts.Add(alert);
        }

        var resourceAlerts = await EvaluateResourcePressureAlertsAsync(organizationId, cancellationToken);
        raisedAlerts.AddRange(resourceAlerts);

        await ResolveClearedAlertsAsync(organizationId, raisedAlerts, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return raisedAlerts;
    }

    private async Task<IReadOnlyList<AlertSummary>> EvaluateResourcePressureAlertsAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var alerts = new List<AlertSummary>();
        var now = dateTimeService.UtcNow;
        var cutoff = now.AddHours(-1);

        var pods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId
                && p.Status != PodStatus.Deleted
                && p.Status != PodStatus.Deleting)
            .ToListAsync(cancellationToken);

        if (pods.Count == 0)
        {
            return alerts;
        }

        var podIds = pods.Select(p => p.Id).ToList();
        var providerIds = pods.Select(p => p.ProviderId).Distinct().ToList();

        var recentMetrics = await dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId && m.RecordedAt >= cutoff)
            .ToListAsync(cancellationToken);

        var latestMetrics = recentMetrics
            .GroupBy(m => m.GpuPodId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.RecordedAt).First());

        var models = await dbContext.AiModels
            .Where(m => m.OrganizationId == organizationId
                && podIds.Contains(m.PodId)
                && m.Status != ModelStatus.Deleted)
            .ToListAsync(cancellationToken);

        var modelsByPod = models
            .GroupBy(m => m.PodId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var gpus = await dbContext.ProviderGpus
            .Where(g => providerIds.Contains(g.ComputeProviderId))
            .ToListAsync(cancellationToken);

        foreach (var pod in pods)
        {
            latestMetrics.TryGetValue(pod.Id, out var metric);
            modelsByPod.TryGetValue(pod.Id, out var podModels);
            podModels ??= [];

            var configuredDiskBytes = (long)(pod.ContainerDisk + pod.VolumeDisk) * BytesPerGigabyte;
            var diskUsedBytes = metric?.DiskUsedBytes
                ?? (podModels.Count > 0 ? podModels.Sum(m => m.Size) : null);

            if (configuredDiskBytes > 0 && diskUsedBytes.HasValue && diskUsedBytes.Value > 0)
            {
                var diskPercent = diskUsedBytes.Value * 100.0 / configuredDiskBytes;
                if (diskPercent >= DiskFullThresholdPercent)
                {
                    var alert = await RaiseOrUpdateAlertAsync(
                        organizationId,
                        AlertType.DiskFull,
                        AlertSeverity.Critical,
                        $"Disk full on {pod.Name}",
                        $"Disk usage is {diskPercent:F1}% of configured capacity",
                        gpuPodId: pod.Id,
                        cancellationToken: cancellationToken);
                    alerts.Add(alert);
                }
            }

            var gpuCatalog = gpus.FirstOrDefault(g =>
                g.ComputeProviderId == pod.ProviderId
                && (string.IsNullOrEmpty(pod.GpuId)
                    ? g.GpuType == pod.GpuType
                    : g.GpuId == pod.GpuId || g.GpuType == pod.GpuType));

            var memoryTotalBytes = gpuCatalog?.MemoryGb is > 0
                ? gpuCatalog.MemoryGb.Value * BytesPerGigabyte
                : (long?)null;

            var memoryUsedBytes = metric?.MemoryUsedBytes
                ?? (podModels.Count > 0 ? podModels.Sum(m => m.Size) : null);

            if (memoryTotalBytes.HasValue && memoryUsedBytes.HasValue && memoryUsedBytes.Value > 0)
            {
                var memoryPercent = memoryUsedBytes.Value * 100.0 / memoryTotalBytes.Value;
                if (memoryPercent >= MemoryPressureThresholdPercent)
                {
                    var alert = await RaiseOrUpdateAlertAsync(
                        organizationId,
                        AlertType.MemoryPressure,
                        AlertSeverity.Warning,
                        $"Memory pressure on {pod.Name}",
                        $"Memory usage is {memoryPercent:F1}% of GPU capacity",
                        gpuPodId: pod.Id,
                        cancellationToken: cancellationToken);
                    alerts.Add(alert);
                }
            }
        }

        return alerts;
    }

    private async Task<AlertSummary> RaiseOrUpdateAlertAsync(
        Guid organizationId,
        AlertType alertType,
        AlertSeverity severity,
        string title,
        string message,
        Guid? providerId = null,
        Guid? gpuPodId = null,
        string? modelName = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.AlertHistory
            .Where(a =>
                a.OrganizationId == organizationId
                && a.AlertType == alertType
                && a.IsActive
                && a.ProviderId == providerId
                && a.GpuPodId == gpuPodId
                && a.ModelName == modelName)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            existing.Message = message;
            existing.Severity = severity;
            return MapAlert(existing);
        }

        var alert = new AlertHistory
        {
            OrganizationId = organizationId,
            RaisedAt = dateTimeService.UtcNow,
            AlertType = alertType,
            Severity = severity,
            Title = title,
            Message = message,
            ProviderId = providerId,
            GpuPodId = gpuPodId,
            ModelName = modelName,
            IsActive = true,
        };

        await dbContext.AddAlertHistoryAsync(alert, cancellationToken);

        await notificationService.NotifyAlertRaisedAsync(
            organizationId,
            alert.Id,
            title,
            severity.ToString(),
            cancellationToken);

        logger.LogWarning(
            "Alert raised for organization {OrganizationId}: {AlertType} - {Title}",
            organizationId,
            alertType,
            title);

        return MapAlert(alert);
    }

    private async Task ResolveClearedAlertsAsync(
        Guid organizationId,
        IReadOnlyList<AlertSummary> activeRaised,
        CancellationToken cancellationToken)
    {
        var activeTypes = activeRaised
            .Select(a => (a.AlertType, a.ProviderId, a.GpuPodId, a.ModelName))
            .ToHashSet();

        var currentlyActive = await dbContext.AlertHistory
            .Where(a => a.OrganizationId == organizationId && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var alert in currentlyActive)
        {
            if (!activeTypes.Contains((alert.AlertType, alert.ProviderId, alert.GpuPodId, alert.ModelName)))
            {
                alert.IsActive = false;
                alert.ResolvedAt = dateTimeService.UtcNow;
            }
        }
    }

    private static ObservabilityHealthStatus ResolveOverallStatus(
        PodHealthOverview podOverview,
        ProviderHealthOverview providerOverview)
    {
        if (podOverview.UnhealthyPods > 0 || providerOverview.UnhealthyProviders > 0)
        {
            return ObservabilityHealthStatus.Unhealthy;
        }

        if (podOverview.DegradedPods > 0)
        {
            return ObservabilityHealthStatus.Degraded;
        }

        return ObservabilityHealthStatus.Healthy;
    }

    private static ObservabilityHealthStatus ResolvePodStatus(GpuPod pod, PodHealthMetric? metric)
    {
        if (pod.Status == PodStatus.Failed || metric?.State == OrchestrationPodState.Failed)
        {
            return ObservabilityHealthStatus.Unhealthy;
        }

        if (metric is null || !metric.OllamaHealthy || !metric.ModelsHealthy)
        {
            return ObservabilityHealthStatus.Degraded;
        }

        return ObservabilityHealthStatus.Healthy;
    }

    private async Task<bool> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Organizations.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database health check failed.");
            return false;
        }
    }

    private static AlertSummary MapAlert(AlertHistory alert) =>
        new()
        {
            Id = alert.Id,
            RaisedAt = alert.RaisedAt,
            ResolvedAt = alert.ResolvedAt,
            AlertType = alert.AlertType,
            Severity = alert.Severity,
            Title = alert.Title,
            Message = alert.Message,
            ProviderId = alert.ProviderId,
            GpuPodId = alert.GpuPodId,
            ModelName = alert.ModelName,
            IsActive = alert.IsActive,
        };
}
