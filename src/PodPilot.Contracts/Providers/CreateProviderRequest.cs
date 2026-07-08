namespace PodPilot.Contracts.Providers;

/// <summary>
/// Request to create a compute provider.
/// </summary>
public sealed class CreateProviderRequest
{
    /// <summary>
    /// Gets or sets the internal name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default region.
    /// </summary>
    public string? DefaultRegion { get; set; }

    /// <summary>
    /// Gets or sets the provider API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
