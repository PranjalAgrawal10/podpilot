using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.GetCapacity;

/// <summary>
/// Gets capacity planning data.
/// </summary>
public sealed class GetCapacityQuery : IRequest<CapacityResponse>
{
    /// <summary>Gets or sets an optional pool identifier.</summary>
    public Guid? PoolId { get; init; }
}
