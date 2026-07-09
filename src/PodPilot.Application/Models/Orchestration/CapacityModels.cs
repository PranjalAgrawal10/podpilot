using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Orchestration;

/// <summary>
/// Capacity planning result.
/// </summary>
public sealed class CapacityPlan
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the optional pool identifier.</summary>
    public Guid? PoolId { get; init; }

    /// <summary>Gets or sets the current capacity (0-1).</summary>
    public double CurrentCapacity { get; init; }

    /// <summary>Gets or sets the projected capacity (0-1).</summary>
    public double ProjectedCapacity { get; init; }

    /// <summary>Gets or sets the remaining capacity (0-1).</summary>
    public double RemainingCapacity { get; init; }

    /// <summary>Gets or sets the maximum throughput (requests per second).</summary>
    public double MaximumThroughput { get; init; }

    /// <summary>Gets or sets the suggested scale adjustment.</summary>
    public int SuggestedScale { get; init; }

    /// <summary>Gets or sets the total pod count.</summary>
    public int TotalPods { get; init; }

    /// <summary>Gets or sets the healthy pod count.</summary>
    public int HealthyPods { get; init; }

    /// <summary>Gets or sets the busy pod count.</summary>
    public int BusyPods { get; init; }

    /// <summary>Gets or sets the queue length.</summary>
    public int QueueLength { get; init; }

    /// <summary>Gets or sets the average wait time in milliseconds.</summary>
    public double AverageWaitTimeMs { get; init; }

    /// <summary>Gets or sets the average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets the GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets the concurrent stream count.</summary>
    public int ConcurrentStreams { get; init; }
}

/// <summary>
/// Auto-scaler status summary.
/// </summary>
public sealed class AutoScalerStatus
{
    /// <summary>Gets or sets pool scaling summaries.</summary>
    public IReadOnlyList<PoolScalingStatus> Pools { get; init; } = [];

    /// <summary>Gets or sets recent scaling events.</summary>
    public IReadOnlyList<ScalingEventSummary> RecentEvents { get; init; } = [];
}

/// <summary>
/// Per-pool scaling status.
/// </summary>
public sealed class PoolScalingStatus
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }

    /// <summary>Gets or sets the pool name.</summary>
    public string PoolName { get; init; } = string.Empty;

    /// <summary>Gets or sets the current pod count.</summary>
    public int CurrentPods { get; init; }

    /// <summary>Gets or sets the minimum pods.</summary>
    public int MinPods { get; init; }

    /// <summary>Gets or sets the maximum pods.</summary>
    public int MaxPods { get; init; }

    /// <summary>Gets or sets the warm standby count.</summary>
    public int WarmStandbyCount { get; init; }

    /// <summary>Gets or sets the current utilization (0-1).</summary>
    public double Utilization { get; init; }

    /// <summary>Gets or sets a value indicating whether scale-up is recommended.</summary>
    public bool ScaleUpRecommended { get; init; }

    /// <summary>Gets or sets a value indicating whether scale-down is recommended.</summary>
    public bool ScaleDownRecommended { get; init; }
}

/// <summary>
/// Summary of a scaling event.
/// </summary>
public sealed class ScalingEventSummary
{
    /// <summary>Gets or sets the event identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid? PoolId { get; init; }

    /// <summary>Gets or sets the direction.</summary>
    public ScalingDirection Direction { get; init; }

    /// <summary>Gets or sets the trigger type.</summary>
    public ScalingTriggerType TriggerType { get; init; }

    /// <summary>Gets or sets the reason.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether scaling succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Gets or sets when the event occurred.</summary>
    public DateTime OccurredAt { get; init; }
}

/// <summary>
/// Result of a scaling action.
/// </summary>
public sealed class ScalingActionResult
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }

    /// <summary>Gets or sets the direction.</summary>
    public ScalingDirection Direction { get; init; }

    /// <summary>Gets or sets a value indicating whether scaling succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Gets or sets the pod identifier created or removed.</summary>
    public Guid? PodId { get; init; }

    /// <summary>Gets or sets the reason.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; init; }
}
