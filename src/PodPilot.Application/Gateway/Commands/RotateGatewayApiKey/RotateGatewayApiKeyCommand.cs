using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Commands.RotateGatewayApiKey;

/// <summary>
/// Rotates a gateway API key.
/// </summary>
public sealed class RotateGatewayApiKeyCommand : IRequest<GatewayApiKeyResponse>
{
    /// <summary>
    /// Gets or sets the key identifier.
    /// </summary>
    public Guid KeyId { get; init; }
}
