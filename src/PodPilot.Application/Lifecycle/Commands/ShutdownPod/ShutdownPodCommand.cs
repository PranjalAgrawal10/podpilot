using MediatR;
using PodPilot.Contracts.Lifecycle;

namespace PodPilot.Application.Lifecycle.Commands.ShutdownPod;

/// <summary>
/// Command to shut down a running GPU pod.
/// </summary>
public sealed class ShutdownPodCommand : IRequest<PodShutdownResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets the shutdown reason.
    /// </summary>
    public string Reason { get; init; } = "Manual shutdown requested.";
}
