using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Queries.ListGatewayRequests;

/// <summary>
/// Lists recent gateway requests for the dashboard.
/// </summary>
public sealed class ListGatewayRequestsQuery : IRequest<IReadOnlyList<GatewayRequestSummaryResponse>>
{
    /// <summary>
    /// Gets or sets the maximum number of requests to return.
    /// </summary>
    public int Limit { get; init; } = 50;
}
