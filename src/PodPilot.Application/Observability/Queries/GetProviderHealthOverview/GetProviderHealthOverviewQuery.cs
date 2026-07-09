using MediatR;
using PodPilot.Contracts.Observability;

namespace PodPilot.Application.Observability.Queries.GetProviderHealthOverview;

/// <summary>
/// Gets provider health overview for the current organization.
/// </summary>
public sealed class GetProviderHealthOverviewQuery : IRequest<ProviderHealthOverviewResponse>
{
}
