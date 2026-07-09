using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Broadcasts observability events via SignalR.
/// </summary>
public sealed class ObservabilityNotificationService : IObservabilityNotificationService
{
    private readonly IHubContext<ObservabilityHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservabilityNotificationService"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context.</param>
    public ObservabilityNotificationService(IHubContext<ObservabilityHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyMetricsUpdatedAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "MetricsUpdated", new { }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyCostUpdatedAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "CostUpdated", new { }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyAlertRaisedAsync(
        Guid organizationId,
        Guid alertId,
        string title,
        string severity,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "AlertRaised", new { alertId, title, severity }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyPodHealthChangedAsync(
        Guid organizationId,
        Guid podId,
        string status,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PodHealthChanged", new { podId, status }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyProviderHealthChangedAsync(
        Guid organizationId,
        Guid providerId,
        string status,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "ProviderHealthChanged", new { providerId, status }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyQueueUpdatedAsync(
        Guid organizationId,
        int queueLength,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "QueueUpdated", new { queueLength }, cancellationToken);

    private Task SendAsync(
        Guid organizationId,
        string eventName,
        object payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(ObservabilityHub.GetOrganizationGroupName(organizationId))
            .SendAsync(eventName, new { payload, updatedAt = DateTime.UtcNow }, cancellationToken);
}
