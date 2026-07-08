using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Broadcasts model management events via SignalR.
/// </summary>
public sealed class ModelNotificationService : IModelNotificationService
{
    private readonly IHubContext<ModelHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelNotificationService"/> class.
    /// </summary>
    public ModelNotificationService(IHubContext<ModelHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyDownloadStartedAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        string modelName,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(ModelHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "ModelDownloadStarted",
                new { downloadId, modelId, modelName, startedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyDownloadProgressAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        int progress,
        long? downloadSpeed,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(ModelHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "ModelDownloadProgress",
                new { downloadId, modelId, progress, downloadSpeed, updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyDownloadCompletedAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        bool success,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(ModelHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "ModelDownloadCompleted",
                new { downloadId, modelId, success, completedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyModelDeletedAsync(
        Guid organizationId,
        Guid modelId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(ModelHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "ModelDeleted",
                new { modelId, deletedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyHealthUpdatedAsync(
        Guid organizationId,
        Guid modelId,
        string status,
        int? responseTimeMs,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(ModelHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "HealthUpdated",
                new { modelId, status, responseTimeMs, checkedAt = DateTime.UtcNow },
                cancellationToken);
}

/// <summary>
/// No-op model notification service for tests.
/// </summary>
public sealed class NoOpModelNotificationService : IModelNotificationService
{
    /// <inheritdoc />
    public Task NotifyDownloadStartedAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        string modelName,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyDownloadProgressAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        int progress,
        long? downloadSpeed,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyDownloadCompletedAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        bool success,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyModelDeletedAsync(
        Guid organizationId,
        Guid modelId,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyHealthUpdatedAsync(
        Guid organizationId,
        Guid modelId,
        string status,
        int? responseTimeMs,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
