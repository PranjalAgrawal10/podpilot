using MediatR;

namespace PodPilot.Application.Gateway.Commands.DeleteGatewayRoute;

/// <summary>
/// Deletes a gateway route.
/// </summary>
public sealed class DeleteGatewayRouteCommand : IRequest
{
    /// <summary>
    /// Gets or sets the route identifier.
    /// </summary>
    public Guid RouteId { get; init; }
}
