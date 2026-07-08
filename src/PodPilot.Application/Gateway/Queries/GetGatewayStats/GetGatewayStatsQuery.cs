using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Queries.GetGatewayStats;

/// <summary>
/// Gets gateway dashboard statistics.
/// </summary>
public sealed class GetGatewayStatsQuery : IRequest<GatewayStatsResponse>
{
}
