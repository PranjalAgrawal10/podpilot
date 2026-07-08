using MediatR;
using PodPilot.Contracts.Lifecycle;

namespace PodPilot.Application.Lifecycle.Commands.UpdatePodIdlePolicy;

/// <summary>
/// Command to update a pod idle policy.
/// </summary>
public sealed class UpdatePodIdlePolicyCommand : IRequest<PodIdlePolicyResponse>
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets idle timeout in minutes.
    /// </summary>
    public int IdleTimeoutMinutes { get; init; }

    /// <summary>
    /// Gets or sets grace period in minutes.
    /// </summary>
    public int GracePeriodMinutes { get; init; }

    /// <summary>
    /// Gets or sets whether auto shutdown is enabled.
    /// </summary>
    public bool AutoShutdownEnabled { get; init; }

    /// <summary>
    /// Gets or sets whether auto wake is enabled.
    /// </summary>
    public bool AutoWakeEnabled { get; init; }

    /// <summary>
    /// Gets or sets minimum running time in minutes.
    /// </summary>
    public int MinimumRunningTimeMinutes { get; init; }
}
