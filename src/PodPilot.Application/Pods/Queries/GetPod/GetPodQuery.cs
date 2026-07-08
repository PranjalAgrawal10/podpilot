using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Queries.GetPod;

/// <summary>
/// Query to get a pod by identifier.
/// </summary>
public sealed class GetPodQuery : IRequest<PodResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
