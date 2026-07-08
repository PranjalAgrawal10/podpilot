using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Broadcasts gateway events via SignalR.
/// </summary>
public sealed class GatewayNotificationService : IGatewayNotificationService
{
    private readonly IHubContext<GatewayHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayNotificationService"/> class.
    /// </summary>
    public GatewayNotificationService(IHubContext<GatewayHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyRequestStartedAsync(
        Guid organizationId,
        Guid requestId,
        Guid podId,
        string path,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "GatewayRequestStarted",
                new { requestId, podId, path, startedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyRequestFinishedAsync(
        Guid organizationId,
        Guid requestId,
        string status,
        int totalLatencyMs,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "GatewayRequestFinished",
                new { requestId, status, totalLatencyMs, finishedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyPodWakeAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "GatewayPodWake",
                new { podId, triggeredAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyGatewayErrorAsync(
        Guid organizationId,
        Guid requestId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "GatewayError",
                new { requestId, errorCode, message, occurredAt = DateTime.UtcNow },
                cancellationToken);
}

/// <summary>
/// No-op gateway notification service for tests.
/// </summary>
public sealed class NoOpGatewayNotificationService : IGatewayNotificationService
{
    /// <inheritdoc />
    public Task NotifyRequestStartedAsync(
        Guid organizationId,
        Guid requestId,
        Guid podId,
        string path,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyRequestFinishedAsync(
        Guid organizationId,
        Guid requestId,
        string status,
        int totalLatencyMs,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPodWakeAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyGatewayErrorAsync(
        Guid organizationId,
        Guid requestId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
