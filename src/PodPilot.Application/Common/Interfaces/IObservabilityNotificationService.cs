namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Broadcasts observability events via SignalR.
/// </summary>
public interface IObservabilityNotificationService
{
    /// <summary>Notifies that metrics were updated.</summary>
    Task NotifyMetricsUpdatedAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that cost data was updated.</summary>
    Task NotifyCostUpdatedAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that an alert was raised.</summary>
    Task NotifyAlertRaisedAsync(Guid organizationId, Guid alertId, string title, string severity, CancellationToken cancellationToken = default);

    /// <summary>Notifies that pod health changed.</summary>
    Task NotifyPodHealthChangedAsync(Guid organizationId, Guid podId, string status, CancellationToken cancellationToken = default);

    /// <summary>Notifies that provider health changed.</summary>
    Task NotifyProviderHealthChangedAsync(Guid organizationId, Guid providerId, string status, CancellationToken cancellationToken = default);

    /// <summary>Notifies that queue metrics were updated.</summary>
    Task NotifyQueueUpdatedAsync(Guid organizationId, int queueLength, CancellationToken cancellationToken = default);
}
