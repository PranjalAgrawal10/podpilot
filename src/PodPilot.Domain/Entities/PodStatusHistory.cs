using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Records a pod status change over time.
/// </summary>
public class PodStatusHistory : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the recorded status.
    /// </summary>
    public PodStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the status was recorded.
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Gets or sets an optional message from the provider.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets the associated GPU pod.
    /// </summary>
    public GpuPod GpuPod { get; set; } = null!;
}
