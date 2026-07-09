using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.ListScalingEvents;

/// <summary>
/// Lists scaling events for the current organization.
/// </summary>
public sealed class ListScalingEventsQuery : IRequest<IReadOnlyList<ScalingEventResponse>>
{
    /// <summary>Gets or sets an optional pool identifier filter.</summary>
    public Guid? PoolId { get; init; }

    /// <summary>Gets or sets the maximum number of records.</summary>
    public int Limit { get; init; } = 50;
}
