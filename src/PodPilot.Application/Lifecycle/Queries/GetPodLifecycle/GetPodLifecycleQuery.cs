using MediatR;
using PodPilot.Contracts.Lifecycle;

namespace PodPilot.Application.Lifecycle.Queries.GetPodLifecycle;

/// <summary>
/// Query to list lifecycle events and summary for a pod.
/// </summary>
public sealed class GetPodLifecycleQuery : IRequest<PodLifecycleSummaryResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
