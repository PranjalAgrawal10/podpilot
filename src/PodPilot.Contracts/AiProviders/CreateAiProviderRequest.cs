namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// Request to create an AI inference provider.
/// </summary>
public sealed class CreateAiProviderRequest
{
    /// <summary>Gets or sets the internal name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the provider kind.</summary>
    public string ProviderKind { get; set; } = string.Empty;

    /// <summary>Gets or sets the API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional base URL override.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Gets or sets an optional deployment name.</summary>
    public string? DeploymentName { get; set; }

    /// <summary>Gets or sets an optional API version.</summary>
    public string? ApiVersion { get; set; }

    /// <summary>Gets or sets a value indicating whether the provider is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets routing priority.</summary>
    public int Priority { get; set; }
}
