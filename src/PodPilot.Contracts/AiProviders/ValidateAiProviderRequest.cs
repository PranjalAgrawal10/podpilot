namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// Request to validate AI provider credentials before or after create.
/// </summary>
public sealed class ValidateAiProviderRequest
{
    /// <summary>Gets or sets the provider kind.</summary>
    public string ProviderKind { get; set; } = string.Empty;

    /// <summary>Gets or sets the API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional base URL.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Gets or sets an optional deployment name.</summary>
    public string? DeploymentName { get; set; }

    /// <summary>Gets or sets an optional API version.</summary>
    public string? ApiVersion { get; set; }
}
