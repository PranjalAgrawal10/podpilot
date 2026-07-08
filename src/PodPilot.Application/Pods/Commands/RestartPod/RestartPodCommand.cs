using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Commands.RestartPod;

/// <summary>
/// Command to restart a GPU pod.
/// </summary>
public sealed class RestartPodCommand : IRequest<PodResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
