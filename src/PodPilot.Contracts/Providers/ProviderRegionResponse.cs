namespace PodPilot.Contracts.Providers;

/// <summary>
/// Provider region response.
/// </summary>
public sealed class ProviderRegionResponse
{
    /// <summary>
    /// Gets or sets the region identifier.
    /// </summary>
    public string RegionId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the region name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the region is available.
    /// </summary>
    public bool IsAvailable { get; init; }
}
