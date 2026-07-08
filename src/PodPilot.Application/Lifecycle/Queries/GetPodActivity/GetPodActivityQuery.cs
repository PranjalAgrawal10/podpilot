using MediatR;
using PodPilot.Contracts.Lifecycle;

namespace PodPilot.Application.Lifecycle.Queries.GetPodActivity;

/// <summary>
/// Query to list activity for a pod.
/// </summary>
public sealed class GetPodActivityQuery : IRequest<IReadOnlyList<PodActivityResponse>>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
