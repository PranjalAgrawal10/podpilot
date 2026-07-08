using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Records activity against a GPU pod for idle detection.
/// </summary>
public class PodActivity : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    public PodActivityType ActivityType { get; set; }

    /// <summary>
    /// Gets or sets when the activity occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the activity source (e.g. api, worker, user).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who triggered the activity, if any.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets optional JSON metadata.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets the associated pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;
}
