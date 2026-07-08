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
}
