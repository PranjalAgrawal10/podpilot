using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// SignalR notifications for scheduler events.
/// </summary>
public sealed class SchedulerNotificationService : ISchedulerNotificationService
{
    private readonly IHubContext<GatewayHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerNotificationService"/> class.
    /// </summary>
    public SchedulerNotificationService(IHubContext<GatewayHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyRequestQueuedAsync(
        Guid organizationId,
        Guid requestId,
        int position,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync("RequestQueued", new { requestId, position }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyRequestStartedAsync(
        Guid organizationId,
        Guid requestId,
        Guid podId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync("RequestStarted", new { requestId, podId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyRequestStreamingAsync(
        Guid organizationId,
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync("RequestStreaming", new { requestId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyRequestCompletedAsync(
        Guid organizationId,
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync("RequestCompleted", new { requestId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyRequestFailedAsync(
        Guid organizationId,
        Guid requestId,
        string error,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync("RequestFailed", new { requestId, error }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyQueueUpdatedAsync(
        Guid organizationId,
        int queueLength,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(GatewayHub.GetOrganizationGroupName(organizationId))
            .SendAsync("QueueUpdated", new { queueLength }, cancellationToken);
}

/// <summary>
/// No-op scheduler notifications for testing.
/// </summary>
public sealed class NoOpSchedulerNotificationService : ISchedulerNotificationService
{
    /// <inheritdoc />
    public Task NotifyRequestQueuedAsync(Guid organizationId, Guid requestId, int position, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyRequestStartedAsync(Guid organizationId, Guid requestId, Guid podId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyRequestStreamingAsync(Guid organizationId, Guid requestId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyRequestCompletedAsync(Guid organizationId, Guid requestId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyRequestFailedAsync(Guid organizationId, Guid requestId, string error, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyQueueUpdatedAsync(Guid organizationId, int queueLength, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
