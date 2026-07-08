using MediatR;
using PodPilot.Contracts.Lifecycle;

namespace PodPilot.Application.Lifecycle.Queries.GetPodLifecycleEvents;

/// <summary>
/// Query to list lifecycle events for a pod.
/// </summary>
public sealed class GetPodLifecycleEventsQuery : IRequest<IReadOnlyList<PodLifecycleEventResponse>>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
