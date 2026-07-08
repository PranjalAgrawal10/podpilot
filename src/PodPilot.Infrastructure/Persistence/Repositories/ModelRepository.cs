using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IModelRepository"/>.
/// </summary>
public sealed class ModelRepository : IModelRepository
{
    private readonly ApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRepository"/> class.
    /// </summary>
    public ModelRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<AiModel?> GetByIdAsync(Guid organizationId, Guid modelId, CancellationToken cancellationToken = default) =>
        dbContext.AiModels
            .Include(m => m.Pod)
            .Include(m => m.Downloads)
            .Where(m => m.Id == modelId && m.OrganizationId == organizationId && m.Status != ModelStatus.Deleted)
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public Task<AiModel?> GetByReferenceAsync(
        Guid organizationId,
        Guid podId,
        string name,
        string tag,
        CancellationToken cancellationToken = default) =>
        dbContext.AiModels
            .Include(m => m.Pod)
            .Where(m => m.OrganizationId == organizationId
                && m.PodId == podId
                && m.Name == name
                && m.Tag == tag
                && m.Status != ModelStatus.Deleted)
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiModel>> ListAsync(
        Guid organizationId,
        Guid? podId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AiModels
            .Include(m => m.Pod)
            .Where(m => m.OrganizationId == organizationId && m.Status != ModelStatus.Deleted);

        if (podId.HasValue)
        {
            query = query.Where(m => m.PodId == podId.Value);
        }

        return await query
            .OrderByDescending(m => m.IsDefault)
            .ThenBy(m => m.Name)
            .ThenBy(m => m.Tag)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelDownload>> ListActiveDownloadsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        await dbContext.ModelDownloads
            .Include(d => d.Model)
                .ThenInclude(m => m.Pod)
            .Where(d => d.Model.OrganizationId == organizationId
                && (d.Status == ModelDownloadStatus.Queued || d.Status == ModelDownloadStatus.Downloading))
            .OrderByDescending(d => d.StartedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<ModelHealthHistory?> GetLatestHealthAsync(Guid modelId, CancellationToken cancellationToken = default) =>
        dbContext.ModelHealthHistoryEntries
            .Where(h => h.ModelId == modelId)
            .OrderByDescending(h => h.LastChecked)
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelHealthHistory>> ListHealthHistoryAsync(
        Guid modelId,
        int limit = 20,
        CancellationToken cancellationToken = default) =>
        await dbContext.ModelHealthHistoryEntries
            .Include(h => h.Model)
            .Where(h => h.ModelId == modelId)
            .OrderByDescending(h => h.LastChecked)
            .Take(limit)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task AddModelAsync(AiModel model, CancellationToken cancellationToken = default) =>
        dbContext.AiModels.AddAsync(model, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddDownloadAsync(ModelDownload download, CancellationToken cancellationToken = default) =>
        dbContext.ModelDownloads.AddAsync(download, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddHealthHistoryAsync(ModelHealthHistory history, CancellationToken cancellationToken = default) =>
        dbContext.ModelHealthHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
