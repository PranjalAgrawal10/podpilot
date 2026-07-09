using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Records an auto-scaling event.
/// </summary>
public class ScalingEvent : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the pod pool identifier.
    /// </summary>
    public Guid? PodPoolId { get; set; }

    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the scaling direction.
    /// </summary>
    public ScalingDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets the trigger type.
    /// </summary>
    public ScalingTriggerType TriggerType { get; set; }

    /// <summary>
    /// Gets or sets the reason for scaling.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether scaling succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets the pod count before scaling.
    /// </summary>
    public int PodCountBefore { get; set; }

    /// <summary>
    /// Gets or sets the pod count after scaling.
    /// </summary>
    public int PodCountAfter { get; set; }
}
