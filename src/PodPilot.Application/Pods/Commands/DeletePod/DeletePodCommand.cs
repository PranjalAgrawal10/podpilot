using MediatR;

namespace PodPilot.Application.Pods.Commands.DeletePod;

/// <summary>
/// Command to delete a GPU pod.
/// </summary>
public sealed class DeletePodCommand : IRequest
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to force delete a running pod.
    /// </summary>
    public bool Force { get; init; }
}
