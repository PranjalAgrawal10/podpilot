namespace PodPilot.Application.Models.Lifecycle;

/// <summary>
/// Idle status evaluation for a GPU pod.
/// </summary>
public sealed class PodIdleStatus
{
    /// <summary>
    /// Gets or sets whether the pod is currently idle.
    /// </summary>
    public bool IsIdle { get; init; }

    /// <summary>
    /// Gets or sets idle duration in minutes.
    /// </summary>
    public double IdleMinutes { get; init; }

    /// <summary>
    /// Gets or sets when idle was first detected.
    /// </summary>
    public DateTime? IdleDetectedAt { get; init; }

    /// <summary>
    /// Gets or sets when shutdown is scheduled.
    /// </summary>
    public DateTime? NextShutdownAt { get; init; }

    /// <summary>
    /// Gets or sets when the pod last recorded activity.
    /// </summary>
    public DateTime? LastActivityAt { get; init; }
}
