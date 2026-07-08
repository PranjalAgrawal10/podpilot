namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a region available on a compute provider.
/// </summary>
public class ProviderRegion : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the compute provider identifier.
    /// </summary>
    public Guid ComputeProviderId { get; set; }

    /// <summary>
    /// Gets or sets the provider-specific region identifier.
    /// </summary>
    public string RegionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable region name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the region is available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Gets or sets when this region was last synced.
    /// </summary>
    public DateTime SyncedAt { get; set; }

    /// <summary>
    /// Gets the compute provider.
    /// </summary>
    public ComputeProvider ComputeProvider { get; set; } = null!;
}
