namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Lifecycle summary for dashboard display.
/// </summary>
public sealed class PodLifecycleSummaryResponse
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets current pod status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets running time in minutes.
    /// </summary>
    public double RunningTimeMinutes { get; init; }

    /// <summary>
    /// Gets or sets idle time in minutes.
    /// </summary>
    public double IdleTimeMinutes { get; init; }

    /// <summary>
    /// Gets or sets last activity timestamp.
    /// </summary>
    public DateTime? LastActivityAt { get; init; }

    /// <summary>
    /// Gets or sets next scheduled shutdown.
    /// </summary>
    public DateTime? NextShutdownAt { get; init; }

    /// <summary>
    /// Gets or sets whether auto wake is enabled.
    /// </summary>
    public bool AutoWakeEnabled { get; init; }

    /// <summary>
    /// Gets or sets whether auto shutdown is enabled.
    /// </summary>
    public bool AutoShutdownEnabled { get; init; }

    /// <summary>
    /// Gets or sets whether the pod is currently idle.
    /// </summary>
    public bool IsIdle { get; init; }

    /// <summary>
    /// Gets or sets the idle policy.
    /// </summary>
    public PodIdlePolicyResponse Policy { get; init; } = null!;
}
