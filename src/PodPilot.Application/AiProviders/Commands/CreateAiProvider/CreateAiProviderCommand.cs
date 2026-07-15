using MediatR;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.CreateAiProvider;

/// <summary>
/// Creates an AI inference provider for the current organization.
/// </summary>
public sealed class CreateAiProviderCommand : IRequest<AiProviderResponse>
{
    /// <summary>Gets or sets the internal name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the provider kind.</summary>
    public AiProviderKind ProviderKind { get; init; }

    /// <summary>Gets or sets the API key.</summary>
    public string ApiKey { get; init; } = string.Empty;

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
