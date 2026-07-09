using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Broadcasts orchestration events via SignalR.
/// </summary>
public sealed class OrchestratorNotificationService : IOrchestratorNotificationService
{
    private readonly IHubContext<OrchestratorHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrchestratorNotificationService"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context.</param>
    public OrchestratorNotificationService(IHubContext<OrchestratorHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyPodAddedAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PodAdded", new { poolId, podId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyPodRemovedAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PodRemoved", new { poolId, podId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyScalingStartedAsync(
        Guid organizationId,
        Guid poolId,
        string direction,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "ScalingStarted", new { poolId, direction }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyScalingCompletedAsync(
        Guid organizationId,
        Guid poolId,
        bool success,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "ScalingCompleted", new { poolId, success }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyPodFailedAsync(
        Guid organizationId,
        Guid podId,
        string reason,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PodFailed", new { podId, reason }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyFailoverTriggeredAsync(
        Guid organizationId,
        Guid failedPodId,
        Guid? replacementPodId,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            organizationId,
            "FailoverTriggered",
            new { failedPodId, replacementPodId },
            cancellationToken);

    /// <inheritdoc />
    public Task NotifyPoolUpdatedAsync(
        Guid organizationId,
        Guid poolId,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PoolUpdated", new { poolId }, cancellationToken);

    private Task SendAsync(
        Guid organizationId,
        string eventName,
        object payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(OrchestratorHub.GetOrganizationGroupName(organizationId))
            .SendAsync(eventName, new { payload, updatedAt = DateTime.UtcNow }, cancellationToken);
}
