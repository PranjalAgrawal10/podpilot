using MediatR;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.ValidateAiProvider;

/// <summary>
/// Validates AI provider credentials.
/// </summary>
public sealed class ValidateAiProviderCommand : IRequest<AiProviderValidationResponse>
{
    /// <summary>Gets or sets an optional existing provider identifier.</summary>
    public Guid? ProviderId { get; init; }

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
}
