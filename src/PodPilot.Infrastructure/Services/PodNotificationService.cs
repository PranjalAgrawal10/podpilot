using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Broadcasts pod status updates via SignalR.
/// </summary>
public sealed class PodNotificationService : IPodNotificationService
{
    private readonly IHubContext<PodStatusHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodNotificationService"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context.</param>
    public PodNotificationService(IHubContext<PodStatusHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyPodStatusChangedAsync(
        Guid organizationId,
        Guid podId,
        string status,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(PodStatusHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "PodStatusChanged",
                new { podId, status, updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyLifecycleEventAsync(
        Guid organizationId,
        Guid podId,
        string eventName,
        object? payload = null,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(PodStatusHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                eventName,
                new { podId, payload, updatedAt = DateTime.UtcNow },
                cancellationToken);
}
