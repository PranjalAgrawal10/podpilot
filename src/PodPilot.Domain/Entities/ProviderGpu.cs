using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a GPU type available on a compute provider.
/// </summary>
public class ProviderGpu : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the compute provider identifier.
    /// </summary>
    public Guid ComputeProviderId { get; set; }

    /// <summary>
    /// Gets or sets the mapped GPU type.
    /// </summary>
    public GpuType GpuType { get; set; }

    /// <summary>
    /// Gets or sets the provider-specific GPU identifier.
    /// </summary>
    public string GpuId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU memory in gigabytes.
    /// </summary>
    public int? MemoryGb { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the GPU is available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Gets or sets when this GPU was last synced.
    /// </summary>
    public DateTime SyncedAt { get; set; }

    /// <summary>
    /// Gets the compute provider.
    /// </summary>
    public ComputeProvider ComputeProvider { get; set; } = null!;
}
