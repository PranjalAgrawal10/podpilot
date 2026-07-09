using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// No-op observability notification service for testing environments.
/// </summary>
public sealed class NoOpObservabilityNotificationService : IObservabilityNotificationService
{
    /// <inheritdoc />
    public Task NotifyMetricsUpdatedAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyCostUpdatedAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyAlertRaisedAsync(
        Guid organizationId,
        Guid alertId,
        string title,
        string severity,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPodHealthChangedAsync(
        Guid organizationId,
        Guid podId,
        string status,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyProviderHealthChangedAsync(
        Guid organizationId,
        Guid providerId,
        string status,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyQueueUpdatedAsync(
        Guid organizationId,
        int queueLength,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
