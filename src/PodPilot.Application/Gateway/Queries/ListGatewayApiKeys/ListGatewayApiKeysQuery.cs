using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Queries.ListGatewayApiKeys;

/// <summary>
/// Lists gateway API keys for the current organization.
/// </summary>
public sealed class ListGatewayApiKeysQuery : IRequest<IReadOnlyList<GatewayApiKeyResponse>>
{
}
