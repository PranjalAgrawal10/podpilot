namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Broadcasts model management events via SignalR.
/// </summary>
public interface IModelNotificationService
{
    /// <summary>
    /// Notifies that a model download started.
    /// </summary>
    Task NotifyDownloadStartedAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies download progress.
    /// </summary>
    Task NotifyDownloadProgressAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        int progress,
        long? downloadSpeed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a download completed.
    /// </summary>
    Task NotifyDownloadCompletedAsync(
        Guid organizationId,
        Guid downloadId,
        Guid modelId,
        bool success,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a model was deleted.
    /// </summary>
    Task NotifyModelDeletedAsync(
        Guid organizationId,
        Guid modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that model health was updated.
    /// </summary>
    Task NotifyHealthUpdatedAsync(
        Guid organizationId,
        Guid modelId,
        string status,
        int? responseTimeMs,
        CancellationToken cancellationToken = default);
}
