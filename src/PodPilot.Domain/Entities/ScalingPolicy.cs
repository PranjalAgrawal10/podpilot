namespace PodPilot.Domain.Entities;

/// <summary>
/// Auto-scaling policy for a pod pool or organization.
/// </summary>
public class ScalingPolicy : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the policy name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum number of pods.
    /// </summary>
    public int MinPods { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of pods.
    /// </summary>
    public int MaxPods { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum queue length before scale-up.
    /// </summary>
    public int MaxQueueLength { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum acceptable latency in milliseconds.
    /// </summary>
    public int MaxLatencyMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the scale-up utilization threshold (0-1).
    /// </summary>
    public double ScaleUpThreshold { get; set; } = 0.8;

    /// <summary>
    /// Gets or sets the scale-down utilization threshold (0-1).
    /// </summary>
    public double ScaleDownThreshold { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the number of warm standby pods to maintain.
    /// </summary>
    public int WarmStandbyCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minimum runtime in minutes before scale-down.
    /// </summary>
    public int MinRuntimeMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether auto scale-up is enabled.
    /// </summary>
    public bool AutoScaleUpEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether auto scale-down is enabled.
    /// </summary>
    public bool AutoScaleDownEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the evaluation interval in seconds.
    /// </summary>
    public int EvaluationIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;
}
