using MediatR;
using PodPilot.Contracts.AiProviders;

namespace PodPilot.Application.AiProviders.Commands.UpdateAiProvider;

/// <summary>
/// Updates an AI inference provider.
/// </summary>
public sealed class UpdateAiProviderCommand : IRequest<AiProviderResponse>
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the internal name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets an optional API key replacement.</summary>
    public string? ApiKey { get; init; }

    /// <summary>Gets or sets an optional base URL.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Gets or sets an optional deployment name.</summary>
    public string? DeploymentName { get; init; }

    /// <summary>Gets or sets an optional API version.</summary>
    public string? ApiVersion { get; init; }

    /// <summary>Gets or sets a value indicating whether the provider is enabled.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Gets or sets routing priority.</summary>
    public int Priority { get; init; }
}
