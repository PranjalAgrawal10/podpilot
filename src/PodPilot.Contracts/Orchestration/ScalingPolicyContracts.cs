namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Scaling policy request/response.
/// </summary>
public class ScalingPolicyRequest
{
    /// <summary>Gets or sets the policy name.</summary>
    public string Name { get; set; } = "Default";

    /// <summary>Gets or sets the minimum pods.</summary>
    public int MinPods { get; set; } = 1;

    /// <summary>Gets or sets the maximum pods.</summary>
    public int MaxPods { get; set; } = 10;

    /// <summary>Gets or sets the maximum queue length.</summary>
    public int MaxQueueLength { get; set; } = 50;

    /// <summary>Gets or sets the maximum latency in milliseconds.</summary>
    public int MaxLatencyMs { get; set; } = 5000;

    /// <summary>Gets or sets the scale-up threshold.</summary>
    public double ScaleUpThreshold { get; set; } = 0.8;

    /// <summary>Gets or sets the scale-down threshold.</summary>
    public double ScaleDownThreshold { get; set; } = 0.3;

    /// <summary>Gets or sets the warm standby count.</summary>
    public int WarmStandbyCount { get; set; } = 1;

    /// <summary>Gets or sets the minimum runtime in minutes.</summary>
    public int MinRuntimeMinutes { get; set; } = 10;

    /// <summary>Gets or sets a value indicating whether auto scale-up is enabled.</summary>
    public bool AutoScaleUpEnabled { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether auto scale-down is enabled.</summary>
    public bool AutoScaleDownEnabled { get; set; } = true;
}

/// <summary>
/// Scaling policy response.
/// </summary>
public sealed class ScalingPolicyResponse : ScalingPolicyRequest
{
    /// <summary>Gets or sets the policy identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the evaluation interval in seconds.</summary>
    public int EvaluationIntervalSeconds { get; set; } = 60;
}
