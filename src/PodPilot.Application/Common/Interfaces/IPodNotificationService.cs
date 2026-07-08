namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Broadcasts real-time pod status updates to connected clients.
/// </summary>
public interface IPodNotificationService
{
    /// <summary>
    /// Notifies clients that a pod status has changed.
    /// </summary>
    Task NotifyPodStatusChangedAsync(
        Guid organizationId,
        Guid podId,
        string status,
        CancellationToken cancellationToken = default);
}
