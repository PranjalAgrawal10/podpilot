namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Broadcasts real-time gateway events to connected clients.
/// </summary>
public interface IGatewayNotificationService
{
    /// <summary>
    /// Notifies that a gateway request started.
    /// </summary>
    Task NotifyRequestStartedAsync(
        Guid organizationId,
        Guid requestId,
        Guid podId,
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a gateway request finished.
    /// </summary>
    Task NotifyRequestFinishedAsync(
        Guid organizationId,
        Guid requestId,
        string status,
        int totalLatencyMs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a pod wake was triggered by the gateway.
    /// </summary>
    Task NotifyPodWakeAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies of a gateway error.
    /// </summary>
    Task NotifyGatewayErrorAsync(
        Guid organizationId,
        Guid requestId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default);
}
