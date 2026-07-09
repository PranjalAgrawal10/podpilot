using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.GetPodPool;

/// <summary>
/// Gets a pod pool by identifier.
/// </summary>
public sealed class GetPodPoolQuery : IRequest<PodPoolResponse>
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }
}
