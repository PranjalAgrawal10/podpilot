using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Application.Models.Ollama;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Ollama;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Orchestrates Ollama model operations on GPU pods.
/// </summary>
public sealed class ModelService : IModelService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IModelRepository modelRepository;
    private readonly IOllamaClient ollamaClient;
    private readonly IInferenceClient inferenceClient;
    private readonly IPodLifecycleService podLifecycleService;
    private readonly IModelNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ModelService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelService"/> class.
    /// </summary>
    public ModelService(
        IApplicationDbContext dbContext,
        IModelRepository modelRepository,
        IOllamaClient ollamaClient,
        IInferenceClient inferenceClient,
        IPodLifecycleService podLifecycleService,
        IModelNotificationService notificationService,
        IDateTimeService dateTimeService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ModelService> logger)
    {
        this.dbContext = dbContext;
        this.modelRepository = modelRepository;
        this.ollamaClient = ollamaClient;
        this.inferenceClient = inferenceClient;
        this.podLifecycleService = podLifecycleService;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<GpuPod> EnsurePodReadyAsync(
        Guid organizationId,
        Guid podId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var pod = await dbContext.GpuPods
            .Include(p => p.Endpoints)
            .Where(p => p.Id == podId && p.OrganizationId == organizationId && p.Status != PodStatus.Deleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Pod", podId);

        if (pod.Status != PodStatus.Running)
        {
            logger.LogInformation("Waking pod {PodId} for model operation", podId);
            var wakeResult = await podLifecycleService.WakePodAsync(
                podId,
                organizationId,
                "model-management",
                userId,
                processImmediately: true,
                cancellationToken);

            if (!wakeResult.Success)
            {
                throw new ValidationException(wakeResult.ErrorMessage ?? "Failed to wake pod for model operation.");
            }

            pod = await dbContext.GpuPods
                .Include(p => p.Endpoints)
                .FirstAsync(p => p.Id == podId, cancellationToken);
        }

        var baseUrl = GetOllamaBaseUrl(pod);
        var healthy = await inferenceClient.WaitForHealthyAsync(baseUrl, cancellationToken);
        if (!healthy)
        {
            throw new ValidationException("Ollama is not reachable on the selected pod.");
        }

        return pod;
    }

    /// <inheritdoc />
    public string GetOllamaBaseUrl(GpuPod pod) => OllamaUrlHelper.GetOllamaBaseUrl(pod);

    /// <inheritdoc />
    public async Task<ModelDownloadResponse> StartPullAsync(
        Guid organizationId,
        Guid podId,
        string modelReference,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var pod = await EnsurePodReadyAsync(organizationId, podId, userId, cancellationToken);
        var (name, tag) = ModelReferenceParser.Parse(modelReference);
        var fullName = ModelReferenceParser.ToReference(name, tag);

        var existing = await modelRepository.GetByReferenceAsync(organizationId, podId, name, tag, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == ModelStatus.Available)
            {
                throw new ValidationException($"Model '{fullName}' is already installed on this pod.");
            }

            if (existing.Status == ModelStatus.Downloading)
            {
                throw new ValidationException($"Model '{fullName}' is already being downloaded.");
            }
        }

        var activeDownloads = await modelRepository.ListActiveDownloadsAsync(organizationId, cancellationToken);
        if (activeDownloads.Any(d => d.Model.PodId == podId
            && string.Equals(d.Model.FullName, fullName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"A download for '{fullName}' is already in progress.");
        }

        var now = dateTimeService.UtcNow;
        var model = existing ?? new AiModel
        {
            OrganizationId = organizationId,
            PodId = podId,
            Name = name,
            Tag = tag,
            Status = ModelStatus.Downloading,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        if (existing is not null)
        {
            model.Status = ModelStatus.Downloading;
            model.UpdatedAt = now;
            model.UpdatedBy = userId.ToString();
        }
        else
        {
            await modelRepository.AddModelAsync(model, cancellationToken);
        }

        var download = new ModelDownload
        {
            ModelId = model.Id,
            Progress = 0,
            Status = ModelDownloadStatus.Queued,
            StartedAt = now,
        };

        await modelRepository.AddDownloadAsync(download, cancellationToken);
        await modelRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Pull started for model {ModelName} on pod {PodId} (download {DownloadId})",
            fullName,
            podId,
            download.Id);

        await notificationService.NotifyDownloadStartedAsync(
            organizationId,
            download.Id,
            model.Id,
            fullName,
            cancellationToken);

        var downloadId = download.Id;
        _ = Task.Run(
            async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var scopedService = scope.ServiceProvider.GetRequiredService<IModelService>();
                    await scopedService.ExecutePullAsync(downloadId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Background pull failed for download {DownloadId}", downloadId);
                }
            },
            CancellationToken.None);

        download.Model = model;
        download.Model.Pod = pod;
        return ModelMapper.ToDownloadResponse(download);
    }

    /// <inheritdoc />
    public async Task ExecutePullAsync(Guid downloadId, CancellationToken cancellationToken = default)
    {
        var download = await dbContext.ModelDownloads
            .Include(d => d.Model)
                .ThenInclude(m => m.Pod)
            .FirstOrDefaultAsync(d => d.Id == downloadId, cancellationToken);

        if (download is null)
        {
            return;
        }

        var model = download.Model;
        var pod = model.Pod;
        var baseUrl = GetOllamaBaseUrl(pod);
        var fullName = model.FullName;
        var lastProgressUpdate = dateTimeService.UtcNow;
        var lastCompleted = 0L;
        var lastUpdateTime = dateTimeService.UtcNow;

        download.Status = ModelDownloadStatus.Downloading;
        model.Status = ModelStatus.Downloading;
        await modelRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await ollamaClient.PullModelAsync(
                baseUrl,
                fullName,
                async progress =>
                {
                    var progressPercent = CalculateProgress(progress);
                    download.Progress = progressPercent;
                    download.DownloadSpeed = CalculateSpeed(
                        progress,
                        ref lastCompleted,
                        ref lastUpdateTime,
                        dateTimeService.UtcNow);

                    if (progressPercent == 100 || (dateTimeService.UtcNow - lastProgressUpdate).TotalSeconds >= 1)
                    {
                        lastProgressUpdate = dateTimeService.UtcNow;
                        await modelRepository.SaveChangesAsync(cancellationToken);
                        await notificationService.NotifyDownloadProgressAsync(
                            model.OrganizationId,
                            download.Id,
                            model.Id,
                            download.Progress,
                            download.DownloadSpeed,
                            cancellationToken);
                    }
                },
                cancellationToken);

            var details = await ollamaClient.ShowModelAsync(baseUrl, fullName, cancellationToken);
            ModelMapper.ApplyOllamaDetails(model, details);
            model.Status = ModelStatus.Available;
            model.UpdatedAt = dateTimeService.UtcNow;

            download.Status = ModelDownloadStatus.Completed;
            download.Progress = 100;
            download.CompletedAt = dateTimeService.UtcNow;

            await modelRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Pull completed for model {ModelName} on pod {PodId}", fullName, pod.Id);

            await notificationService.NotifyDownloadCompletedAsync(
                model.OrganizationId,
                download.Id,
                model.Id,
                success: true,
                cancellationToken);
        }
        catch (Exception ex)
        {
            download.Status = ModelDownloadStatus.Failed;
            download.ErrorMessage = ex.Message;
            download.CompletedAt = dateTimeService.UtcNow;
            model.Status = ModelStatus.Failed;
            model.UpdatedAt = dateTimeService.UtcNow;

            await modelRepository.SaveChangesAsync(cancellationToken);

            logger.LogError(ex, "Pull failed for model {ModelName} on pod {PodId}", fullName, pod.Id);

            await notificationService.NotifyDownloadCompletedAsync(
                model.OrganizationId,
                download.Id,
                model.Id,
                success: false,
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteModelAsync(
        Guid organizationId,
        Guid modelId,
        bool forceDefault,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var model = await modelRepository.GetByIdAsync(organizationId, modelId, cancellationToken)
            ?? throw new NotFoundException("Model", modelId);

        if (model.IsDefault && !forceDefault)
        {
            throw new ValidationException("Cannot delete the default model without confirmation.");
        }

        var pod = await EnsurePodReadyAsync(organizationId, model.PodId, userId, cancellationToken);
        var baseUrl = GetOllamaBaseUrl(pod);

        if (model.Status == ModelStatus.Available)
        {
            await ollamaClient.DeleteModelAsync(baseUrl, model.FullName, cancellationToken);
        }

        model.Status = ModelStatus.Deleted;
        model.IsDefault = false;
        model.UpdatedAt = dateTimeService.UtcNow;
        model.UpdatedBy = userId.ToString();

        await modelRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted model {ModelId} ({ModelName}) from pod {PodId}", modelId, model.FullName, pod.Id);

        await notificationService.NotifyModelDeletedAsync(organizationId, modelId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ModelResponse> SetDefaultModelAsync(
        Guid organizationId,
        Guid modelId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var model = await modelRepository.GetByIdAsync(organizationId, modelId, cancellationToken)
            ?? throw new NotFoundException("Model", modelId);

        if (model.Status != ModelStatus.Available)
        {
            throw new ValidationException("Only available models can be set as default.");
        }

        var modelsOnPod = await dbContext.AiModels
            .Where(m => m.OrganizationId == organizationId
                && m.PodId == model.PodId
                && m.Status != ModelStatus.Deleted)
            .ToListAsync(cancellationToken);

        foreach (var candidate in modelsOnPod)
        {
            candidate.IsDefault = candidate.Id == modelId;
            candidate.UpdatedAt = dateTimeService.UtcNow;
            candidate.UpdatedBy = userId.ToString();
        }

        var route = await dbContext.GatewayRoutes
            .Where(r => r.OrganizationId == organizationId && r.GpuPodId == model.PodId && r.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        if (route is not null)
        {
            route.ModelName = model.FullName;
            route.UpdatedAt = dateTimeService.UtcNow;
        }

        await modelRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Default model changed to {ModelName} on pod {PodId}", model.FullName, model.PodId);

        return ModelMapper.ToResponse(model);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelResponse>> RefreshModelsAsync(
        Guid organizationId,
        Guid podId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var pod = await EnsurePodReadyAsync(organizationId, podId, userId, cancellationToken);
        var baseUrl = GetOllamaBaseUrl(pod);
        var tags = await ollamaClient.GetTagsAsync(baseUrl, cancellationToken);
        var now = dateTimeService.UtcNow;

        var existingModels = await dbContext.AiModels
            .Where(m => m.OrganizationId == organizationId && m.PodId == podId && m.Status != ModelStatus.Deleted)
            .ToListAsync(cancellationToken);

        var synced = new List<AiModel>();

        foreach (var tag in tags)
        {
            var (name, parsedTag) = ModelReferenceParser.Parse(tag.Name);
            var model = existingModels.FirstOrDefault(m =>
                m.Name == name && m.Tag == parsedTag);

            if (model is null)
            {
                model = new AiModel
                {
                    OrganizationId = organizationId,
                    PodId = podId,
                    Name = name,
                    Tag = parsedTag,
                    Status = ModelStatus.Available,
                    CreatedAt = now,
                    CreatedBy = userId.ToString(),
                };
                ModelMapper.ApplyOllamaTag(model, tag);

                try
                {
                    var details = await ollamaClient.ShowModelAsync(baseUrl, tag.Name, cancellationToken);
                    ModelMapper.ApplyOllamaDetails(model, details);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not load show metadata for {ModelName}", tag.Name);
                }

                await modelRepository.AddModelAsync(model, cancellationToken);
            }
            else
            {
                ModelMapper.ApplyOllamaTag(model, tag);
                model.Status = ModelStatus.Available;
                model.UpdatedAt = now;
                model.UpdatedBy = userId.ToString();

                try
                {
                    var details = await ollamaClient.ShowModelAsync(baseUrl, tag.Name, cancellationToken);
                    ModelMapper.ApplyOllamaDetails(model, details);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not refresh show metadata for {ModelName}", tag.Name);
                }
            }

            model.Pod = pod;
            synced.Add(model);
        }

        var tagReferences = tags
            .Select(t => ModelReferenceParser.Parse(t.Name))
            .Select(t => (t.Name, t.Tag))
            .ToHashSet();

        foreach (var stale in existingModels.Where(m => m.Status == ModelStatus.Available))
        {
            if (!tagReferences.Contains((stale.Name, stale.Tag)))
            {
                stale.Status = ModelStatus.Deleted;
                stale.IsDefault = false;
                stale.UpdatedAt = now;
                stale.UpdatedBy = userId.ToString();
            }
        }

        await modelRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Refreshed {Count} models from Ollama on pod {PodId}", synced.Count, podId);

        return synced.Select(ModelMapper.ToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<(bool Detected, string? Version)> TryDetectOllamaAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default)
    {
        var pod = await dbContext.GpuPods
            .Include(p => p.Endpoints)
            .Where(p => p.Id == podId && p.OrganizationId == organizationId && p.Status == PodStatus.Running)
            .FirstOrDefaultAsync(cancellationToken);

        if (pod is null)
        {
            return (false, null);
        }

        try
        {
            var baseUrl = GetOllamaBaseUrl(pod);
            if (!await ollamaClient.IsReachableAsync(baseUrl, cancellationToken))
            {
                return (false, null);
            }

            var version = await ollamaClient.GetVersionAsync(baseUrl, cancellationToken);
            return (true, version.Version);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Ollama detection failed for pod {PodId}", podId);
            return (false, null);
        }
    }

    private static int CalculateProgress(OllamaPullProgress progress)
    {
        if (progress.Total is > 0 && progress.Completed is not null)
        {
            return (int)Math.Clamp(progress.Completed.Value * 100 / progress.Total.Value, 0, 100);
        }

        return progress.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ? 100 : 0;
    }

    private static long? CalculateSpeed(
        OllamaPullProgress progress,
        ref long lastCompleted,
        ref DateTime lastUpdateTime,
        DateTime now)
    {
        if (progress.Completed is null || progress.Total is null)
        {
            return null;
        }

        var elapsed = (now - lastUpdateTime).TotalSeconds;
        if (elapsed <= 0)
        {
            return null;
        }

        var delta = progress.Completed.Value - lastCompleted;
        lastCompleted = progress.Completed.Value;
        lastUpdateTime = now;
        return delta > 0 ? (long)(delta / elapsed) : null;
    }
}
