namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Auto-scaler status response.
/// </summary>
public sealed class AutoScalerStatusResponse
{
    /// <summary>Gets or sets per-pool scaling status.</summary>
    public IReadOnlyList<PoolScalingStatusResponse> Pools { get; set; } = [];

    /// <summary>Gets or sets recent scaling events.</summary>
    public IReadOnlyList<ScalingEventResponse> RecentEvents { get; set; } = [];
}

/// <summary>
/// Per-pool scaling status response.
/// </summary>
public sealed class PoolScalingStatusResponse
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; set; }

    /// <summary>Gets or sets the pool name.</summary>
    public string PoolName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current pod count.</summary>
    public int CurrentPods { get; set; }

    /// <summary>Gets or sets the minimum pods.</summary>
    public int MinPods { get; set; }

    /// <summary>Gets or sets the maximum pods.</summary>
    public int MaxPods { get; set; }

    /// <summary>Gets or sets the warm standby count.</summary>
    public int WarmStandbyCount { get; set; }

    /// <summary>Gets or sets utilization (0-1).</summary>
    public double Utilization { get; set; }

    /// <summary>Gets or sets a value indicating whether scale-up is recommended.</summary>
    public bool ScaleUpRecommended { get; set; }

    /// <summary>Gets or sets a value indicating whether scale-down is recommended.</summary>
    public bool ScaleDownRecommended { get; set; }
}
