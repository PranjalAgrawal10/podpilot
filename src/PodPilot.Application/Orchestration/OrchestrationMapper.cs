using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Orchestration;

/// <summary>
/// Maps orchestration entities to contract responses.
/// </summary>
internal static class OrchestrationMapper
{
    /// <summary>
    /// Maps a pod pool to a response DTO.
    /// </summary>
    public static PodPoolResponse ToResponse(PodPool pool) =>
        new()
        {
            Id = pool.Id,
            OrganizationId = pool.OrganizationId,
            Name = pool.Name,
            Description = pool.Description,
            PoolType = pool.PoolType.ToString(),
            IsDefault = pool.IsDefault,
            IsActive = pool.IsActive,
            ProviderId = pool.ProviderId,
            GpuId = pool.GpuId,
            GpuType = pool.GpuType?.ToString(),
            Region = pool.Region,
            TemplateId = pool.TemplateId,
            ImageName = pool.ImageName,
            ScalingPolicyId = pool.ScalingPolicyId,
            ScalingPolicy = pool.ScalingPolicy is null ? null : ToScalingPolicyResponse(pool.ScalingPolicy),
            Models = pool.Models.Select(m => m.ModelName).ToList(),
            Members = pool.Members.Select(ToMemberResponse).ToList(),
            CreatedAt = pool.CreatedAt,
            UpdatedAt = pool.UpdatedAt,
        };

    /// <summary>
    /// Maps a pool member to a response DTO.
    /// </summary>
    public static PodPoolMemberResponse ToMemberResponse(PodPoolMember member) =>
        new()
        {
            Id = member.Id,
            GpuPodId = member.GpuPodId,
            PodName = member.GpuPod?.Name ?? string.Empty,
            PodStatus = member.GpuPod?.Status.ToString() ?? string.Empty,
            State = member.State.ToString(),
            Weight = member.Weight,
            IsWarmStandby = member.IsWarmStandby,
            ActiveStreams = member.ActiveStreams,
            JoinedAt = member.JoinedAt,
            LastHealthCheckAt = member.LastHealthCheckAt,
        };

    /// <summary>
    /// Maps a scaling policy to a response DTO.
    /// </summary>
    public static ScalingPolicyResponse ToScalingPolicyResponse(ScalingPolicy policy) =>
        new()
        {
            Id = policy.Id,
            Name = policy.Name,
            MinPods = policy.MinPods,
            MaxPods = policy.MaxPods,
            MaxQueueLength = policy.MaxQueueLength,
            MaxLatencyMs = policy.MaxLatencyMs,
            ScaleUpThreshold = policy.ScaleUpThreshold,
            ScaleDownThreshold = policy.ScaleDownThreshold,
            WarmStandbyCount = policy.WarmStandbyCount,
            MinRuntimeMinutes = policy.MinRuntimeMinutes,
            AutoScaleUpEnabled = policy.AutoScaleUpEnabled,
            AutoScaleDownEnabled = policy.AutoScaleDownEnabled,
            EvaluationIntervalSeconds = policy.EvaluationIntervalSeconds,
        };

    /// <summary>
    /// Maps a scaling event to a response DTO.
    /// </summary>
    public static ScalingEventResponse ToScalingEventResponse(ScalingEvent scalingEvent) =>
        new()
        {
            Id = scalingEvent.Id,
            PodPoolId = scalingEvent.PodPoolId,
            GpuPodId = scalingEvent.GpuPodId,
            Direction = scalingEvent.Direction.ToString(),
            TriggerType = scalingEvent.TriggerType.ToString(),
            Reason = scalingEvent.Reason,
            Success = scalingEvent.Success,
            ErrorMessage = scalingEvent.ErrorMessage,
            OccurredAt = scalingEvent.OccurredAt,
            PodCountBefore = scalingEvent.PodCountBefore,
            PodCountAfter = scalingEvent.PodCountAfter,
        };

    /// <summary>
    /// Maps a health metric to a response DTO.
    /// </summary>
    public static PodHealthMetricResponse ToHealthMetricResponse(PodHealthMetric metric) =>
        new()
        {
            Id = metric.Id,
            GpuPodId = metric.GpuPodId,
            RecordedAt = metric.RecordedAt,
            GpuHealthy = metric.GpuHealthy,
            OllamaHealthy = metric.OllamaHealthy,
            ModelsHealthy = metric.ModelsHealthy,
            LatencyMs = metric.LatencyMs,
            GpuUtilizationPercent = metric.GpuUtilizationPercent,
            MemoryUsedBytes = metric.MemoryUsedBytes,
            DiskUsedBytes = metric.DiskUsedBytes,
            NetworkHealthy = metric.NetworkHealthy,
            State = metric.State.ToString(),
            ErrorMessage = metric.ErrorMessage,
        };

    /// <summary>
    /// Maps capacity plan to response DTO.
    /// </summary>
    public static CapacityResponse ToCapacityResponse(Models.Orchestration.CapacityPlan plan) =>
        new()
        {
            OrganizationId = plan.OrganizationId,
            PoolId = plan.PoolId,
            CurrentCapacity = plan.CurrentCapacity,
            ProjectedCapacity = plan.ProjectedCapacity,
            RemainingCapacity = plan.RemainingCapacity,
            MaximumThroughput = plan.MaximumThroughput,
            SuggestedScale = plan.SuggestedScale,
            TotalPods = plan.TotalPods,
            HealthyPods = plan.HealthyPods,
            BusyPods = plan.BusyPods,
            QueueLength = plan.QueueLength,
            AverageWaitTimeMs = plan.AverageWaitTimeMs,
            AverageLatencyMs = plan.AverageLatencyMs,
            GpuUtilizationPercent = plan.GpuUtilizationPercent,
            ConcurrentStreams = plan.ConcurrentStreams,
        };

    /// <summary>
    /// Maps orchestrator status to response DTO.
    /// </summary>
    public static OrchestratorStatusResponse ToOrchestratorStatusResponse(Models.Orchestration.OrchestratorStatus status) =>
        new()
        {
            PoolCount = status.PoolCount,
            RunningPods = status.RunningPods,
            HealthyPods = status.HealthyPods,
            DrainingPods = status.DrainingPods,
            FailedPods = status.FailedPods,
            QueueLength = status.QueueLength,
            AverageLatencyMs = status.AverageLatencyMs,
            RequestsPerSecond = status.RequestsPerSecond,
        };

    /// <summary>
    /// Maps auto-scaler status to response DTO.
    /// </summary>
    public static AutoScalerStatusResponse ToAutoScalerStatusResponse(Models.Orchestration.AutoScalerStatus status) =>
        new()
        {
            Pools = status.Pools.Select(p => new PoolScalingStatusResponse
            {
                PoolId = p.PoolId,
                PoolName = p.PoolName,
                CurrentPods = p.CurrentPods,
                MinPods = p.MinPods,
                MaxPods = p.MaxPods,
                WarmStandbyCount = p.WarmStandbyCount,
                Utilization = p.Utilization,
                ScaleUpRecommended = p.ScaleUpRecommended,
                ScaleDownRecommended = p.ScaleDownRecommended,
            }).ToList(),
            RecentEvents = status.RecentEvents.Select(e => new ScalingEventResponse
            {
                Id = e.Id,
                PodPoolId = e.PoolId,
                Direction = e.Direction.ToString(),
                TriggerType = e.TriggerType.ToString(),
                Reason = e.Reason,
                Success = e.Success,
                OccurredAt = e.OccurredAt,
            }).ToList(),
        };

    /// <summary>
    /// Maps load balancer config to response DTO.
    /// </summary>
    public static LoadBalancerConfigResponse ToLoadBalancerConfigResponse(Models.Orchestration.LoadBalancerConfigDto config) =>
        new()
        {
            Strategy = config.Strategy.ToString(),
            StickySessionsEnabled = config.StickySessionsEnabled,
            StickySessionTtlMinutes = config.StickySessionTtlMinutes,
        };

    /// <summary>
    /// Parses a pool type from string.
    /// </summary>
    public static PodPoolType ParsePoolType(string value) =>
        Enum.TryParse<PodPoolType>(value, ignoreCase: true, out var poolType)
            ? poolType
            : PodPoolType.Custom;

    /// <summary>
    /// Parses a load balancing strategy from string.
    /// </summary>
    public static LoadBalancingStrategy ParseLoadBalancingStrategy(string value) =>
        Enum.TryParse<LoadBalancingStrategy>(value, ignoreCase: true, out var strategy)
            ? strategy
            : LoadBalancingStrategy.LeastBusy;
}
