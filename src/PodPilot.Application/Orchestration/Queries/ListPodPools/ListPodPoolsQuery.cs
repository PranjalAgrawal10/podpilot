using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.ListPodPools;

/// <summary>
/// Lists pod pools for the current organization.
/// </summary>
public sealed class ListPodPoolsQuery : IRequest<IReadOnlyList<PodPoolResponse>>
{
}
