using MediatR;

namespace PodPilot.Application.Gateway.Commands.RevokeGatewayApiKey;

/// <summary>
/// Revokes a gateway API key.
/// </summary>
public sealed class RevokeGatewayApiKeyCommand : IRequest
{
    /// <summary>
    /// Gets or sets the key identifier.
    /// </summary>
    public Guid KeyId { get; init; }
}
