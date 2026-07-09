using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Orchestrator;

/// <summary>
/// Evaluates scaling policies and triggers scale up/down actions.
/// </summary>
public sealed class AutoScalerService : IAutoScaler
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPodPoolManager podPoolManager;
    private readonly ICapacityPlanner capacityPlanner;
    private readonly IPodService podService;
    private readonly IPodLifecycleService lifecycleService;
    private readonly IInferenceClient inferenceClient;
    private readonly IOrchestratorNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<AutoScalerService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoScalerService"/> class.
    /// </summary>
    public AutoScalerService(
        IApplicationDbContext dbContext,
        IPodPoolManager podPoolManager,
        ICapacityPlanner capacityPlanner,
        IPodService podService,
        IPodLifecycleService lifecycleService,
        IInferenceClient inferenceClient,
        IOrchestratorNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<AutoScalerService> logger)
    {
        this.dbContext = dbContext;
        this.podPoolManager = podPoolManager;
        this.capacityPlanner = capacityPlanner;
        this.podService = podService;
        this.lifecycleService = lifecycleService;
        this.inferenceClient = inferenceClient;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScalingActionResult>> EvaluateAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ScalingActionResult>();
        var pools = await dbContext.PodPools
            .Where(p => p.OrganizationId == organizationId && p.IsActive)
            .Include(p => p.ScalingPolicy)
            .ToListAsync(cancellationToken);

        foreach (var pool in pools)
        {
            if (pool.ScalingPolicy is null)
            {
                continue;
            }

            try
            {
                var plan = await capacityPlanner.CalculateAsync(organizationId, pool.Id, cancellationToken);
                var currentPods = plan.TotalPods;
                var policy = pool.ScalingPolicy;

                if (policy.AutoScaleUpEnabled
                    && currentPods < policy.MaxPods
                    && ShouldScaleUp(plan, policy))
                {
                    var reason = BuildScaleUpReason(plan, policy);
                    results.Add(await ScaleUpInternalAsync(
                        organizationId,
                        pool,
                        policy,
                        ScalingTriggerType.Automatic,
                        reason,
                        cancellationToken));
                }
                else if (policy.AutoScaleDownEnabled
                    && currentPods > policy.MinPods
                    && ShouldScaleDown(plan, policy))
                {
                    var reason = BuildScaleDownReason(plan, policy);
                    results.Add(await ScaleDownInternalAsync(
                        organizationId,
                        pool,
                        policy,
                        ScalingTriggerType.Automatic,
                        reason,
                        cancellationToken));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Auto-scaler evaluation failed for pool {PoolId}", pool.Id);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public Task<ScalingActionResult> ScaleUpAsync(
        Guid organizationId,
        Guid poolId,
        string reason,
        CancellationToken cancellationToken = default) =>
        ScaleUpInternalAsync(
            organizationId,
            poolId,
            ScalingTriggerType.Manual,
            reason,
            cancellationToken);

    /// <inheritdoc />
    public Task<ScalingActionResult> ScaleDownAsync(
        Guid organizationId,
        Guid poolId,
        string reason,
        CancellationToken cancellationToken = default) =>
        ScaleDownInternalAsync(
            organizationId,
            poolId,
            ScalingTriggerType.Manual,
            reason,
            cancellationToken);

    /// <inheritdoc />
    public async Task<AutoScalerStatus> GetStatusAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var pools = await dbContext.PodPools
            .Where(p => p.OrganizationId == organizationId && p.IsActive)
            .Include(p => p.ScalingPolicy)
            .Include(p => p.Members)
            .ToListAsync(cancellationToken);

        var poolStatuses = new List<PoolScalingStatus>();

        foreach (var pool in pools)
        {
            var plan = await capacityPlanner.CalculateAsync(organizationId, pool.Id, cancellationToken);
            var policy = pool.ScalingPolicy;
            var warmStandbyCount = pool.Members.Count(m => m.IsWarmStandby);

            poolStatuses.Add(new PoolScalingStatus
            {
                PoolId = pool.Id,
                PoolName = pool.Name,
                CurrentPods = plan.TotalPods,
                MinPods = policy?.MinPods ?? 1,
                MaxPods = policy?.MaxPods ?? plan.TotalPods,
                WarmStandbyCount = warmStandbyCount,
                Utilization = plan.CurrentCapacity,
                ScaleUpRecommended = policy is not null && ShouldScaleUp(plan, policy),
                ScaleDownRecommended = policy is not null && ShouldScaleDown(plan, policy),
            });
        }

        var recentEvents = await dbContext.ScalingEvents
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(20)
            .Select(e => new ScalingEventSummary
            {
                Id = e.Id,
                PoolId = e.PodPoolId,
                Direction = e.Direction,
                TriggerType = e.TriggerType,
                Reason = e.Reason,
                Success = e.Success,
                OccurredAt = e.OccurredAt,
            })
            .ToListAsync(cancellationToken);

        return new AutoScalerStatus
        {
            Pools = poolStatuses,
            RecentEvents = recentEvents,
        };
    }

    private async Task<ScalingActionResult> ScaleUpAsync(
        Guid organizationId,
        Guid poolId,
        ScalingTriggerType triggerType,
        string reason,
        CancellationToken cancellationToken)
    {
        var pool = await dbContext.PodPools
            .Include(p => p.ScalingPolicy)
            .FirstOrDefaultAsync(p => p.Id == poolId && p.OrganizationId == organizationId, cancellationToken)
            ?? throw new InvalidOperationException($"Pod pool {poolId} was not found.");

        return await ScaleUpInternalAsync(
            organizationId,
            pool,
            pool.ScalingPolicy,
            triggerType,
            reason,
            cancellationToken);
    }

    private Task<ScalingActionResult> ScaleUpInternalAsync(
        Guid organizationId,
        Guid poolId,
        ScalingTriggerType triggerType,
        string reason,
        CancellationToken cancellationToken) =>
        ScaleUpAsync(organizationId, poolId, triggerType, reason, cancellationToken);

    private async Task<ScalingActionResult> ScaleUpInternalAsync(
        Guid organizationId,
        PodPool pool,
        ScalingPolicy? policy,
        ScalingTriggerType triggerType,
        string reason,
        CancellationToken cancellationToken)
    {
        var podCountBefore = await dbContext.PodPoolMembers.CountAsync(m => m.PodPoolId == pool.Id, cancellationToken);

        if (policy is not null && podCountBefore >= policy.MaxPods)
        {
            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleUp,
                Success = false,
                Reason = reason,
                ErrorMessage = "Pool has reached maximum pod count.",
            };
        }

        if (pool.ProviderId is null
            || string.IsNullOrWhiteSpace(pool.GpuId)
            || string.IsNullOrWhiteSpace(pool.Region)
            || string.IsNullOrWhiteSpace(pool.ImageName))
        {
            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleUp,
                Success = false,
                Reason = reason,
                ErrorMessage = "Pool does not have a provider template configured for auto-provisioning.",
            };
        }

        await notificationService.NotifyScalingStartedAsync(
            organizationId,
            pool.Id,
            ScalingDirection.ScaleUp.ToString(),
            cancellationToken);

        try
        {
            var provider = await dbContext.ComputeProviders
                .Include(p => p.Credential)
                .FirstOrDefaultAsync(
                    p => p.Id == pool.ProviderId && p.OrganizationId == organizationId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Pool provider was not found.");

            var now = dateTimeService.UtcNow;
            var podName = BuildScaledPodName(pool.Name, now);
            var gpuType = pool.GpuType ?? GpuType.RTX4090;

            var pod = new GpuPod
            {
                OrganizationId = organizationId,
                ProviderId = provider.Id,
                Name = podName,
                Status = PodStatus.BuildingPending,
                GpuType = gpuType,
                GpuId = pool.GpuId!,
                Region = pool.Region!,
                TemplateId = pool.TemplateId,
                ImageName = pool.ImageName!,
                ContainerDisk = ApplicationConstants.PodMinContainerDiskGb,
                VolumeDisk = 0,
                IsPublic = true,
                CreatedAt = now,
                CreatedBy = "auto-scaler",
            };

            var ollamaPort = $"{ApplicationConstants.OllamaPort}/http";
            var configuration = new PodConfiguration
            {
                TemplateId = pool.TemplateId,
                ImageName = pod.ImageName,
                ContainerDiskGb = ApplicationConstants.PodMinContainerDiskGb,
                VolumeDiskGb = 0,
                VolumeMountPath = "/workspace",
                GpuCount = 1,
                PortsJson = System.Text.Json.JsonSerializer.Serialize(new[] { ollamaPort }),
                EnablePublicIp = true,
            };

            pod.Configuration = configuration;

            var createOptions = new PodCreateOptions
            {
                Name = pod.Name,
                GpuId = pod.GpuId,
                GpuType = pod.GpuType,
                Region = pod.Region,
                TemplateId = pod.TemplateId,
                ImageName = pod.ImageName,
                ContainerDiskGb = configuration.ContainerDiskGb,
                VolumeDiskGb = configuration.VolumeDiskGb,
                VolumeMountPath = configuration.VolumeMountPath,
                GpuCount = configuration.GpuCount,
                Ports = [ollamaPort],
                EnablePublicIp = true,
            };

            var providerInfo = await podService.CreatePodAsync(provider, createOptions, cancellationToken);
            podService.ApplyProviderInfo(pod, providerInfo, now);
            pod.UpdatedAt = now;
            pod.UpdatedBy = "auto-scaler";

            await dbContext.AddGpuPodAsync(pod, cancellationToken);
            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = now,
                    Message = "Auto-scaler provisioned pod.",
                },
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            var baseUrl = Gateway.GatewayUrlHelper.GetOllamaBaseUrl(pod);
            var healthy = await inferenceClient.WaitForHealthyAsync(baseUrl, cancellationToken);
            if (!healthy)
            {
                throw new InvalidOperationException("Provisioned pod did not become healthy in time.");
            }

            await podPoolManager.AddMemberAsync(organizationId, pool.Id, pod.Id, cancellationToken: cancellationToken);
            await podPoolManager.UpdateMemberStateAsync(pool.Id, pod.Id, OrchestrationPodState.Healthy, cancellationToken);

            var podCountAfter = podCountBefore + 1;
            await RecordScalingEventAsync(
                organizationId,
                pool.Id,
                pod.Id,
                ScalingDirection.ScaleUp,
                triggerType,
                reason,
                success: true,
                podCountBefore,
                podCountAfter,
                cancellationToken: cancellationToken);

            await notificationService.NotifyScalingCompletedAsync(organizationId, pool.Id, true, cancellationToken);

            logger.LogInformation(
                "Scaled up pool {PoolId} with pod {PodId} for organization {OrganizationId}",
                pool.Id,
                pod.Id,
                organizationId);

            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleUp,
                Success = true,
                PodId = pod.Id,
                Reason = reason,
            };
        }
        catch (Exception ex)
        {
            await RecordScalingEventAsync(
                organizationId,
                pool.Id,
                null,
                ScalingDirection.ScaleUp,
                triggerType,
                reason,
                success: false,
                podCountBefore,
                podCountBefore,
                ex.Message,
                cancellationToken);

            await notificationService.NotifyScalingCompletedAsync(organizationId, pool.Id, false, cancellationToken);

            logger.LogWarning(ex, "Scale-up failed for pool {PoolId}", pool.Id);

            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleUp,
                Success = false,
                Reason = reason,
                ErrorMessage = ex.Message,
            };
        }
    }

    private async Task<ScalingActionResult> ScaleDownInternalAsync(
        Guid organizationId,
        Guid poolId,
        ScalingTriggerType triggerType,
        string reason,
        CancellationToken cancellationToken)
    {
        var pool = await dbContext.PodPools
            .Include(p => p.ScalingPolicy)
            .FirstOrDefaultAsync(p => p.Id == poolId && p.OrganizationId == organizationId, cancellationToken)
            ?? throw new InvalidOperationException($"Pod pool {poolId} was not found.");

        return await ScaleDownInternalAsync(organizationId, pool, pool.ScalingPolicy, triggerType, reason, cancellationToken);
    }

    private async Task<ScalingActionResult> ScaleDownInternalAsync(
        Guid organizationId,
        PodPool pool,
        ScalingPolicy? policy,
        ScalingTriggerType triggerType,
        string reason,
        CancellationToken cancellationToken)
    {
        var podCountBefore = await dbContext.PodPoolMembers.CountAsync(m => m.PodPoolId == pool.Id, cancellationToken);

        if (policy is not null && podCountBefore <= policy.MinPods)
        {
            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleDown,
                Success = false,
                Reason = reason,
                ErrorMessage = "Pool is at minimum pod count.",
            };
        }

        var candidate = await SelectScaleDownCandidateAsync(pool.Id, cancellationToken);
        if (candidate is null)
        {
            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleDown,
                Success = false,
                Reason = reason,
                ErrorMessage = "No eligible pod available for scale-down.",
            };
        }

        if (await podPoolManager.HasActiveStreamsAsync(organizationId, candidate.GpuPodId, cancellationToken))
        {
            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleDown,
                Success = false,
                Reason = reason,
                ErrorMessage = "Cannot scale down while pod has active requests.",
            };
        }

        await notificationService.NotifyScalingStartedAsync(
            organizationId,
            pool.Id,
            ScalingDirection.ScaleDown.ToString(),
            cancellationToken);

        try
        {
            await podPoolManager.StartDrainingAsync(organizationId, pool.Id, candidate.GpuPodId, cancellationToken);

            var drainDeadline = dateTimeService.UtcNow.AddMinutes(5);
            while (dateTimeService.UtcNow < drainDeadline)
            {
                if (!await podPoolManager.HasActiveStreamsAsync(organizationId, candidate.GpuPodId, cancellationToken))
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            if (await podPoolManager.HasActiveStreamsAsync(organizationId, candidate.GpuPodId, cancellationToken))
            {
                await podPoolManager.UpdateMemberStateAsync(pool.Id, candidate.GpuPodId, OrchestrationPodState.Busy, cancellationToken);
                throw new InvalidOperationException("Pod still has active requests; scale-down aborted.");
            }

            var shutdownResult = await lifecycleService.ShutdownPodAsync(
                candidate.GpuPodId,
                organizationId,
                "auto-scaler",
                reason,
                cancellationToken: cancellationToken);

            if (!shutdownResult.Success)
            {
                throw new InvalidOperationException(shutdownResult.ErrorMessage ?? "Pod shutdown failed.");
            }

            await podPoolManager.RemoveMemberAsync(organizationId, pool.Id, candidate.GpuPodId, cancellationToken);

            var podCountAfter = podCountBefore - 1;
            await RecordScalingEventAsync(
                organizationId,
                pool.Id,
                candidate.GpuPodId,
                ScalingDirection.ScaleDown,
                triggerType,
                reason,
                success: true,
                podCountBefore,
                podCountAfter,
                cancellationToken: cancellationToken);

            await notificationService.NotifyScalingCompletedAsync(organizationId, pool.Id, true, cancellationToken);

            logger.LogInformation(
                "Scaled down pool {PoolId}, removed pod {PodId} for organization {OrganizationId}",
                pool.Id,
                candidate.GpuPodId,
                organizationId);

            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleDown,
                Success = true,
                PodId = candidate.GpuPodId,
                Reason = reason,
            };
        }
        catch (Exception ex)
        {
            await RecordScalingEventAsync(
                organizationId,
                pool.Id,
                candidate.GpuPodId,
                ScalingDirection.ScaleDown,
                triggerType,
                reason,
                success: false,
                podCountBefore,
                podCountBefore,
                ex.Message,
                cancellationToken);

            await notificationService.NotifyScalingCompletedAsync(organizationId, pool.Id, false, cancellationToken);

            logger.LogWarning(ex, "Scale-down failed for pool {PoolId}", pool.Id);

            return new ScalingActionResult
            {
                PoolId = pool.Id,
                Direction = ScalingDirection.ScaleDown,
                Success = false,
                PodId = candidate.GpuPodId,
                Reason = reason,
                ErrorMessage = ex.Message,
            };
        }
    }

    private async Task<PodPoolMember?> SelectScaleDownCandidateAsync(Guid poolId, CancellationToken cancellationToken)
    {
        var members = await dbContext.PodPoolMembers
            .Where(m => m.PodPoolId == poolId)
            .Where(m => m.State != OrchestrationPodState.Draining && m.State != OrchestrationPodState.Deleting)
            .Include(m => m.GpuPod)
            .ToListAsync(cancellationToken);

        return members
            .Where(m => !m.IsWarmStandby)
            .OrderBy(m => m.ActiveStreams)
            .ThenByDescending(m => m.JoinedAt)
            .FirstOrDefault()
            ?? members.OrderBy(m => m.ActiveStreams).ThenByDescending(m => m.JoinedAt).FirstOrDefault();
    }

    private static bool ShouldScaleUp(CapacityPlan plan, ScalingPolicy policy) =>
        plan.QueueLength >= policy.MaxQueueLength
        || plan.AverageLatencyMs >= policy.MaxLatencyMs
        || plan.CurrentCapacity >= policy.ScaleUpThreshold
        || plan.GpuUtilizationPercent >= policy.ScaleUpThreshold * 100
        || plan.ConcurrentStreams >= plan.HealthyPods * ApplicationConstants.SchedulerMaxConcurrentPerPod;

    private static bool ShouldScaleDown(CapacityPlan plan, ScalingPolicy policy) =>
        plan.CurrentCapacity <= policy.ScaleDownThreshold
        && plan.QueueLength == 0
        && plan.ConcurrentStreams == 0;

    private static string BuildScaleUpReason(CapacityPlan plan, ScalingPolicy policy)
    {
        if (plan.QueueLength >= policy.MaxQueueLength)
        {
            return $"Queue length {plan.QueueLength} exceeded threshold {policy.MaxQueueLength}.";
        }

        if (plan.AverageLatencyMs >= policy.MaxLatencyMs)
        {
            return $"Average latency {plan.AverageLatencyMs:F0}ms exceeded threshold {policy.MaxLatencyMs}ms.";
        }

        if (plan.GpuUtilizationPercent >= policy.ScaleUpThreshold * 100)
        {
            return $"GPU utilization {plan.GpuUtilizationPercent:F1}% exceeded threshold.";
        }

        return $"Capacity utilization {plan.CurrentCapacity:P0} exceeded scale-up threshold.";
    }

    private static string BuildScaleDownReason(CapacityPlan plan, ScalingPolicy policy) =>
        $"Capacity utilization {plan.CurrentCapacity:P0} below scale-down threshold {policy.ScaleDownThreshold:P0}.";

    private static string BuildScaledPodName(string poolName, DateTime now)
    {
        var suffix = now.ToString("yyyyMMddHHmmss");
        var candidate = $"{poolName}-scale-{suffix}";
        return candidate.Length <= ApplicationConstants.PodNameMaxLength
            ? candidate
            : candidate[..ApplicationConstants.PodNameMaxLength];
    }

    private async Task RecordScalingEventAsync(
        Guid organizationId,
        Guid poolId,
        Guid? podId,
        ScalingDirection direction,
        ScalingTriggerType triggerType,
        string reason,
        bool success,
        int podCountBefore,
        int podCountAfter,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddScalingEventAsync(
            new ScalingEvent
            {
                OrganizationId = organizationId,
                PodPoolId = poolId,
                GpuPodId = podId,
                Direction = direction,
                TriggerType = triggerType,
                Reason = reason,
                Success = success,
                ErrorMessage = errorMessage,
                OccurredAt = dateTimeService.UtcNow,
                PodCountBefore = podCountBefore,
                PodCountAfter = podCountAfter,
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
