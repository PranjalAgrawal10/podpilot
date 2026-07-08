using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Commands.StopPod;

/// <summary>
/// Command to stop a GPU pod.
/// </summary>
public sealed class StopPodCommand : IRequest<PodResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
