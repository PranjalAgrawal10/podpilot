namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Broadcasts orchestration events via SignalR.
/// </summary>
public interface IOrchestratorNotificationService
{
    /// <summary>Notifies that a pod was added to a pool.</summary>
    Task NotifyPodAddedAsync(Guid organizationId, Guid poolId, Guid podId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a pod was removed from a pool.</summary>
    Task NotifyPodRemovedAsync(Guid organizationId, Guid poolId, Guid podId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that scaling has started.</summary>
    Task NotifyScalingStartedAsync(Guid organizationId, Guid poolId, string direction, CancellationToken cancellationToken = default);

    /// <summary>Notifies that scaling has completed.</summary>
    Task NotifyScalingCompletedAsync(Guid organizationId, Guid poolId, bool success, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a pod has failed.</summary>
    Task NotifyPodFailedAsync(Guid organizationId, Guid podId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Notifies that failover was triggered.</summary>
    Task NotifyFailoverTriggeredAsync(Guid organizationId, Guid failedPodId, Guid? replacementPodId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a pool was updated.</summary>
    Task NotifyPoolUpdatedAsync(Guid organizationId, Guid poolId, CancellationToken cancellationToken = default);
}
