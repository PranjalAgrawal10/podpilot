namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// AI inference provider response.
/// </summary>
public sealed class AiProviderResponse
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the internal name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the provider kind.</summary>
    public string ProviderKind { get; init; } = string.Empty;

    /// <summary>Gets or sets the base URL override.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Gets or sets an optional deployment name.</summary>
    public string? DeploymentName { get; init; }

    /// <summary>Gets or sets an optional API version.</summary>
    public string? ApiVersion { get; init; }

    /// <summary>Gets or sets a value indicating whether the provider is enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets or sets a value indicating whether credentials were validated.</summary>
    public bool IsValidated { get; init; }

    /// <summary>Gets or sets when credentials were last validated.</summary>
    public DateTime? LastValidatedAt { get; init; }

    /// <summary>Gets or sets routing priority (lower is preferred).</summary>
    public int Priority { get; init; }

    /// <summary>Gets or sets when the provider was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets when the provider was last updated.</summary>
    public DateTime? UpdatedAt { get; init; }
}
