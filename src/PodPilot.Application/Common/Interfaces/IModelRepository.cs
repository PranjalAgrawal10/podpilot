using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Persistence abstraction for AI models.
/// </summary>
public interface IModelRepository
{
    /// <summary>
    /// Gets a model by identifier scoped to an organization.
    /// </summary>
    Task<AiModel?> GetByIdAsync(Guid organizationId, Guid modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a model by pod and reference name.
    /// </summary>
    Task<AiModel?> GetByReferenceAsync(
        Guid organizationId,
        Guid podId,
        string name,
        string tag,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists models for an organization.
    /// </summary>
    Task<IReadOnlyList<AiModel>> ListAsync(
        Guid organizationId,
        Guid? podId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active downloads for an organization.
    /// </summary>
    Task<IReadOnlyList<ModelDownload>> ListActiveDownloadsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest health record for a model.
    /// </summary>
    Task<ModelHealthHistory?> GetLatestHealthAsync(Guid modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists health history for a model.
    /// </summary>
    Task<IReadOnlyList<ModelHealthHistory>> ListHealthHistoryAsync(
        Guid modelId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a model.
    /// </summary>
    Task AddModelAsync(AiModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a download record.
    /// </summary>
    Task AddDownloadAsync(ModelDownload download, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a health history record.
    /// </summary>
    Task AddHealthHistoryAsync(ModelHealthHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves pending changes.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
