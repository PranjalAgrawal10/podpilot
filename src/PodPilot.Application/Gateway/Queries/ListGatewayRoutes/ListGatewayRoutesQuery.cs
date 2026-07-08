using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Queries.ListGatewayRoutes;

/// <summary>
/// Lists gateway routes for the current organization.
/// </summary>
public sealed class ListGatewayRoutesQuery : IRequest<IReadOnlyList<GatewayRouteResponse>>
{
}
