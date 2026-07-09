using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Gateway;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically checks pod health for orchestrated pool members.
/// </summary>
public sealed class PodHealthWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<PodHealthWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodHealthWorker"/> class.
    /// </summary>
    public PodHealthWorker(IServiceScopeFactory serviceScopeFactory, ILogger<PodHealthWorker> logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPodsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pod health worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckPodsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var inferenceClient = scope.ServiceProvider.GetRequiredService<IInferenceClient>();
        var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();

        var members = await dbContext.PodPoolMembers
            .Include(m => m.GpuPod)
            .Where(m => m.GpuPod.Status != PodStatus.Deleted && m.GpuPod.Status != PodStatus.Deleting)
            .ToListAsync(cancellationToken);

        foreach (var member in members)
        {
            try
            {
                await CheckMemberAsync(
                    member,
                    dbContext,
                    inferenceClient,
                    dateTimeService,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Pod health check failed for pod {PodId}", member.GpuPodId);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckMemberAsync(
        PodPoolMember member,
        IApplicationDbContext dbContext,
        IInferenceClient inferenceClient,
        IDateTimeService dateTimeService,
        CancellationToken cancellationToken)
    {
        var pod = member.GpuPod;
        var baseUrl = GatewayUrlHelper.GetOllamaBaseUrl(pod);
        var recordedAt = dateTimeService.UtcNow;
        string? errorMessage = null;

        var healthStopwatch = Stopwatch.StartNew();
        var ollamaHealthy = false;
        var networkHealthy = false;

        try
        {
            ollamaHealthy = await inferenceClient.IsHealthyAsync(
                baseUrl,
                cancellationToken,
                ApplicationConstants.OllamaQuickHealthCheckTimeout);
            networkHealthy = ollamaHealthy;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            networkHealthy = false;
        }

        healthStopwatch.Stop();
        var latencyMs = (int)healthStopwatch.ElapsedMilliseconds;

        var modelsHealthy = false;
        if (ollamaHealthy)
        {
            try
            {
                var modelsJson = await inferenceClient.GetModelsAsync(baseUrl, cancellationToken);
                using var document = JsonDocument.Parse(modelsJson);
                modelsHealthy = document.RootElement.TryGetProperty("models", out var models)
                    && models.ValueKind == JsonValueKind.Array
                    && models.GetArrayLength() > 0;
            }
            catch (Exception ex)
            {
                errorMessage ??= ex.Message;
            }
        }

        var gpuHealthy = ollamaHealthy && pod.Status == PodStatus.Running;
        var load = member.ActiveStreams;
        var gpuUtilization = load > 0
            ? Math.Min(100, (double)load / ApplicationConstants.SchedulerMaxConcurrentPerPod * 100)
            : (double?)null;

        var state = ResolveState(member, ollamaHealthy, modelsHealthy, load);

        var metric = new PodHealthMetric
        {
            OrganizationId = pod.OrganizationId,
            GpuPodId = pod.Id,
            RecordedAt = recordedAt,
            GpuHealthy = gpuHealthy,
            OllamaHealthy = ollamaHealthy,
            ModelsHealthy = modelsHealthy,
            LatencyMs = latencyMs,
            GpuUtilizationPercent = gpuUtilization,
            MemoryUsedBytes = null,
            DiskUsedBytes = null,
            NetworkHealthy = networkHealthy,
            State = state,
            ErrorMessage = errorMessage,
        };

        await dbContext.AddPodHealthMetricAsync(metric, cancellationToken);

        member.LastHealthCheckAt = recordedAt;
        member.State = state;
        member.ActiveStreams = load;

        if (state == OrchestrationPodState.Failed)
        {
            logger.LogWarning(
                "Pod {PodId} marked failed by health worker: {Error}",
                pod.Id,
                errorMessage ?? "Health checks failed.");
        }
    }

    private static OrchestrationPodState ResolveState(
        PodPoolMember member,
        bool ollamaHealthy,
        bool modelsHealthy,
        int load)
    {
        if (member.State == OrchestrationPodState.Draining)
        {
            return OrchestrationPodState.Draining;
        }

        if (!ollamaHealthy || !modelsHealthy)
        {
            return OrchestrationPodState.Failed;
        }

        return load > 0 ? OrchestrationPodState.Busy : OrchestrationPodState.Healthy;
    }
}
