using MediatR;
using PodPilot.Contracts.Observability;

namespace PodPilot.Application.Observability.Queries.GetLiveMetrics;

/// <summary>
/// Gets live metrics for the current organization dashboard.
/// </summary>
public sealed class GetLiveMetricsQuery : IRequest<LiveMetricsResponse>
{
}
