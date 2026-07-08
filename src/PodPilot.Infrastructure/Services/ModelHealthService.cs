using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Ollama;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Performs health checks against Ollama models.
/// </summary>
public sealed class ModelHealthService : IModelHealthService
{
    private const string HealthPrompt = "ping";

    private readonly IOllamaClient ollamaClient;
    private readonly IModelRepository modelRepository;
    private readonly IModelNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<ModelHealthService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelHealthService"/> class.
    /// </summary>
    public ModelHealthService(
        IOllamaClient ollamaClient,
        IModelRepository modelRepository,
        IModelNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<ModelHealthService> logger)
    {
        this.ollamaClient = ollamaClient;
        this.modelRepository = modelRepository;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<ModelHealthHistory> CheckModelHealthAsync(
        AiModel model,
        GpuPod pod,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = OllamaUrlHelper.GetOllamaBaseUrl(pod);
        var checkedAt = dateTimeService.UtcNow;
        ModelHealthHistory history;

        if (!await ollamaClient.IsReachableAsync(baseUrl, cancellationToken))
        {
            history = CreateHistory(model.Id, ModelHealthStatus.Unavailable, checkedAt, null, "Ollama is not reachable.");
            await PersistAndNotifyAsync(model, history, cancellationToken);
            return history;
        }

        var tags = await ollamaClient.GetTagsAsync(baseUrl, cancellationToken);
        if (!tags.Any(t => string.Equals(t.Name, model.FullName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ModelReferenceParser.Parse(t.Name).Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            history = CreateHistory(model.Id, ModelHealthStatus.ModelMissing, checkedAt, null, "Model is not installed on the pod.");
            await PersistAndNotifyAsync(model, history, cancellationToken);
            return history;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await ollamaClient.GenerateAsync(baseUrl, model.FullName, HealthPrompt, cancellationToken);
            stopwatch.Stop();

            history = CreateHistory(
                model.Id,
                string.IsNullOrWhiteSpace(result.Response) ? ModelHealthStatus.Unhealthy : ModelHealthStatus.Healthy,
                checkedAt,
                (int)stopwatch.ElapsedMilliseconds,
                string.IsNullOrWhiteSpace(result.Response) ? "Generate test returned an empty response." : null);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            history = CreateHistory(model.Id, ModelHealthStatus.Unhealthy, checkedAt, (int)stopwatch.ElapsedMilliseconds, ex.Message);
        }

        await PersistAndNotifyAsync(model, history, cancellationToken);
        logger.LogInformation(
            "Health check for model {ModelName}: {Status} ({ResponseTime}ms)",
            model.FullName,
            history.Status,
            history.ResponseTime);

        return history;
    }

    /// <inheritdoc />
    public async Task CheckPodModelsAsync(GpuPod pod, CancellationToken cancellationToken = default)
    {
        var models = await modelRepository.ListAsync(pod.OrganizationId, pod.Id, cancellationToken);
        foreach (var model in models.Where(m => m.Status == ModelStatus.Available))
        {
            try
            {
                await CheckModelHealthAsync(model, pod, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Health check failed for model {ModelId}", model.Id);
            }
        }
    }

    private async Task PersistAndNotifyAsync(
        AiModel model,
        ModelHealthHistory history,
        CancellationToken cancellationToken)
    {
        await modelRepository.AddHealthHistoryAsync(history, cancellationToken);
        await modelRepository.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyHealthUpdatedAsync(
            model.OrganizationId,
            model.Id,
            history.Status.ToString(),
            history.ResponseTime,
            cancellationToken);
    }

    private static ModelHealthHistory CreateHistory(
        Guid modelId,
        ModelHealthStatus status,
        DateTime checkedAt,
        int? responseTime,
        string? errorMessage) =>
        new()
        {
            ModelId = modelId,
            Status = status,
            ResponseTime = responseTime,
            LastChecked = checkedAt,
            ErrorMessage = errorMessage,
        };
}
