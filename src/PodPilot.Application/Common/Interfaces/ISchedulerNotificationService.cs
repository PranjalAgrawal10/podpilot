namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Broadcasts scheduler events to connected clients.
/// </summary>
public interface ISchedulerNotificationService
{
    /// <summary>
    /// Notifies that a request was queued.
    /// </summary>
    Task NotifyRequestQueuedAsync(
        Guid organizationId,
        Guid requestId,
        int position,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a request started executing.
    /// </summary>
    Task NotifyRequestStartedAsync(
        Guid organizationId,
        Guid requestId,
        Guid podId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a request is streaming.
    /// </summary>
    Task NotifyRequestStreamingAsync(
        Guid organizationId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a request completed.
    /// </summary>
    Task NotifyRequestCompletedAsync(
        Guid organizationId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a request failed.
    /// </summary>
    Task NotifyRequestFailedAsync(
        Guid organizationId,
        Guid requestId,
        string error,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that the queue was updated.
    /// </summary>
    Task NotifyQueueUpdatedAsync(
        Guid organizationId,
        int queueLength,
        CancellationToken cancellationToken = default);
}
