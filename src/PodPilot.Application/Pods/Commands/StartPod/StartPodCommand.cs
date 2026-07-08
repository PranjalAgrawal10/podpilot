using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Commands.StartPod;

/// <summary>
/// Command to start a GPU pod.
/// </summary>
public sealed class StartPodCommand : IRequest<PodResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
