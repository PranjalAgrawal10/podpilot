namespace PodPilot.Domain.Entities;

/// <summary>
/// Idle and auto lifecycle policy for a GPU pod.
/// </summary>
public class PodIdlePolicy : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets minutes of inactivity before a pod is considered idle.
    /// </summary>
    public int IdleTimeoutMinutes { get; set; }

    /// <summary>
    /// Gets or sets grace period minutes after idle detection before shutdown.
    /// </summary>
    public int GracePeriodMinutes { get; set; }

    /// <summary>
    /// Gets or sets whether automatic shutdown is enabled.
    /// </summary>
    public bool AutoShutdownEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether automatic wake is enabled.
    /// </summary>
    public bool AutoWakeEnabled { get; set; }

    /// <summary>
    /// Gets or sets minimum minutes a pod must run before auto shutdown.
    /// </summary>
    public int MinimumRunningTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets when idle was first detected for the current idle period.
    /// </summary>
    public DateTime? IdleDetectedAt { get; set; }

    /// <summary>
    /// Gets the associated pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;
}
