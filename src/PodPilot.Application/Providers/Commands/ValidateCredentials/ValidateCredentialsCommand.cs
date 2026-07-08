using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Commands.ValidateCredentials;

/// <summary>
/// Command to validate provider credentials without a stored provider.
/// </summary>
public sealed class ValidateCredentialsCommand : IRequest<ProviderValidationResponse>
{
    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public Domain.Enums.ProviderType ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the API key to validate.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
