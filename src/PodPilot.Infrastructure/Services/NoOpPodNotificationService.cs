using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// No-op pod notification service for environments without SignalR clients.
/// </summary>
public sealed class NoOpPodNotificationService : IPodNotificationService
{
    /// <inheritdoc />
    public Task NotifyPodStatusChangedAsync(
        Guid organizationId,
        Guid podId,
        string status,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyLifecycleEventAsync(
        Guid organizationId,
        Guid podId,
        string eventName,
        object? payload = null,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
