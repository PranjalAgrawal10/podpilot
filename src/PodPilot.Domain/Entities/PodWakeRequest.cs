using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Queued request to wake a stopped GPU pod.
/// </summary>
public class PodWakeRequest : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the request status.
    /// </summary>
    public PodWakeRequestStatus Status { get; set; } = PodWakeRequestStatus.Pending;

    /// <summary>
    /// Gets or sets the request source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requesting user, if any.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets when the request was created.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Gets or sets when processing started.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }

    /// <summary>
    /// Gets or sets when processing completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets an error message when processing fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the associated pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;
}
