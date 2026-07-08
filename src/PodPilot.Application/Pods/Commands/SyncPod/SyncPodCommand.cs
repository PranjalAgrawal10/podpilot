using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Commands.SyncPod;

/// <summary>
/// Command to synchronize a pod's status with the provider.
/// </summary>
public sealed class SyncPodCommand : IRequest<PodResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
