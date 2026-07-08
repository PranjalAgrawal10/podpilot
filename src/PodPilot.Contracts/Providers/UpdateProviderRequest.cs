namespace PodPilot.Contracts.Providers;

/// <summary>
/// Request to update a compute provider.
/// </summary>
public sealed class UpdateProviderRequest
{
    /// <summary>
    /// Gets or sets the internal name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default region.
    /// </summary>
    public string? DefaultRegion { get; set; }

    /// <summary>
    /// Gets or sets a new API key for rotation.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool? IsEnabled { get; set; }
}
