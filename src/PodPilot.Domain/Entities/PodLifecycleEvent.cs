using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit log of lifecycle engine events for a GPU pod.
/// </summary>
public class PodLifecycleEvent : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public PodLifecycleEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the event source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user associated with the event, if any.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets an optional message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets optional JSON metadata.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets the associated pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;
}
