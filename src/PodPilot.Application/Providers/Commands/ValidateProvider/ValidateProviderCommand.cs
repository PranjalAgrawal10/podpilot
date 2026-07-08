using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Commands.ValidateProvider;

/// <summary>
/// Validates provider credentials.
/// </summary>
public sealed class ValidateProviderCommand : IRequest<ProviderValidationResponse>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets or sets an optional API key to validate instead of the stored credential.
    /// </summary>
    public string? ApiKey { get; init; }
}
