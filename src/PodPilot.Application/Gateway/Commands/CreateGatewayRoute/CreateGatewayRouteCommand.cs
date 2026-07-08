using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Commands.CreateGatewayRoute;

/// <summary>
/// Creates a gateway route.
/// </summary>
public sealed class CreateGatewayRouteCommand : IRequest<GatewayRouteResponse>
{
    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid GpuPodId { get; init; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the default route.
    /// </summary>
    public bool IsDefault { get; init; }
}
