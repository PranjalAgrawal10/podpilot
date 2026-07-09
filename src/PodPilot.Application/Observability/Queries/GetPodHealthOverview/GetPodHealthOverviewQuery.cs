using MediatR;
using PodPilot.Contracts.Observability;

namespace PodPilot.Application.Observability.Queries.GetPodHealthOverview;

/// <summary>
/// Gets pod health overview for the current organization.
/// </summary>
public sealed class GetPodHealthOverviewQuery : IRequest<PodHealthOverviewResponse>
{
}
