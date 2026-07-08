using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Distributed lock for concurrent pod lifecycle operations.
/// </summary>
public class PodLifecycleLock
{
    /// <summary>
    /// Gets or sets the lock identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the locked operation.
    /// </summary>
    public PodLifecycleOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the lock owner identifier.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the lock was acquired.
    /// </summary>
    public DateTime AcquiredAt { get; set; }

    /// <summary>
    /// Gets or sets when the lock expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets the associated pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;
}
