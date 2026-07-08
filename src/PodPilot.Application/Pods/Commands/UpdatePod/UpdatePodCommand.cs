using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Commands.UpdatePod;

/// <summary>
/// Command to update a GPU pod.
/// </summary>
public sealed class UpdatePodCommand : IRequest<PodResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; init; }
}
