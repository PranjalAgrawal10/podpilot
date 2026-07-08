namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Pod idle policy response.
/// </summary>
public sealed class PodIdlePolicyResponse
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

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

    /// <summary>
    /// Gets or sets when idle was first detected.
    /// </summary>
    public DateTime? IdleDetectedAt { get; init; }
}
