namespace PodPilot.Contracts.Providers;

/// <summary>
/// Request to validate provider credentials before saving.
/// </summary>
public sealed class ValidateCredentialsRequest
{
    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key to validate.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional default region.
    /// </summary>
    public string? DefaultRegion { get; set; }
}
