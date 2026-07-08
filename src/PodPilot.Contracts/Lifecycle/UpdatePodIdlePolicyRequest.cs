namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Request to update a pod idle policy.
/// </summary>
public sealed class UpdatePodIdlePolicyRequest
{
    /// <summary>
    /// Gets or sets idle timeout in minutes.
    /// </summary>
    public int IdleTimeoutMinutes { get; init; }

    /// <summary>
    /// Gets or sets grace period in minutes.
    /// </summary>
    public int GracePeriodMinutes { get; init; }

    /// <summary>
    /// Gets or sets whether auto shutdown is enabled.
    /// </summary>
    public bool AutoShutdownEnabled { get; init; }

    /// <summary>
    /// Gets or sets whether auto wake is enabled.
    /// </summary>
    public bool AutoWakeEnabled { get; init; }

    /// <summary>
    /// Gets or sets minimum running time in minutes.
    /// </summary>
    public int MinimumRunningTimeMinutes { get; init; }
}
