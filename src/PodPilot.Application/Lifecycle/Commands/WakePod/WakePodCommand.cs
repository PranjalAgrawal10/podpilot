using MediatR;
using PodPilot.Contracts.Lifecycle;

namespace PodPilot.Application.Lifecycle.Commands.WakePod;

/// <summary>
/// Command to wake a stopped GPU pod.
/// </summary>
public sealed class WakePodCommand : IRequest<PodWakeResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }
}
