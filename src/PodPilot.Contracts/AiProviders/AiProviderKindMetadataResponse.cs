namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// Metadata describing a supported AI provider kind.
/// </summary>
public sealed class AiProviderKindMetadataResponse
{
    /// <summary>Gets or sets the provider kind.</summary>
    public string ProviderKind { get; init; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets the default base URL.</summary>
    public string DefaultBaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets whether a custom base URL is required.</summary>
    public bool RequiresBaseUrl { get; init; }

    /// <summary>Gets or sets whether an API key is required.</summary>
    public bool RequiresApiKey { get; init; }

    /// <summary>Gets or sets whether the provider is OpenAI-compatible.</summary>
    public bool IsOpenAiCompatible { get; init; }
}

/// <summary>
/// Credential validation response for AI providers.
/// </summary>
public sealed class AiProviderValidationResponse
{
    /// <summary>Gets or sets a value indicating whether credentials are valid.</summary>
    public bool IsValid { get; init; }

    /// <summary>Gets or sets an optional message.</summary>
    public string? Message { get; init; }
}
