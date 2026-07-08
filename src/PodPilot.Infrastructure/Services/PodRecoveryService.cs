using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Pods;
using PodPilot.Application.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Replaces failed provider pods with a fresh instance using stored configuration.
/// </summary>
public sealed class PodRecoveryService : IPodRecoveryService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<PodRecoveryService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodRecoveryService"/> class.
    /// </summary>
    public PodRecoveryService(
        IApplicationDbContext dbContext,
        IPodService podService,
        IServiceScopeFactory serviceScopeFactory,
        IDateTimeService dateTimeService,
        ILogger<PodRecoveryService> logger)
    {
        this.dbContext = dbContext;
        this.podService = podService;
        this.serviceScopeFactory = serviceScopeFactory;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<PodRecoveryResult> TryReplacePodOnStartFailureAsync(
        GpuPod pod,
        Guid organizationId,
        string source,
        Guid? userId,
        string? failureReason,
        CancellationToken cancellationToken = default)
    {
        if (pod.Configuration is null)
        {
            return Failure("Pod configuration is missing; automatic replacement is not available.");
        }

        if (string.IsNullOrWhiteSpace(pod.ProviderPodId))
        {
            return Failure("Pod has not been provisioned on the provider.");
        }

        var ownerId = $"replacement:{Guid.NewGuid():N}";
        using var scope = serviceScopeFactory.CreateScope();
        var podLifecycleService = scope.ServiceProvider.GetRequiredService<IPodLifecycleService>();

        if (!await podLifecycleService.TryAcquireLockAsync(
                pod.Id,
                PodLifecycleOperation.Replacement,
                ownerId,
                cancellationToken))
        {
            return Failure("Pod replacement is already in progress.");
        }

        try
        {
            return await ReplacePodAsync(
                pod,
                organizationId,
                source,
                userId,
                failureReason,
                cancellationToken);
        }
        finally
        {
            await podLifecycleService.ReleaseLockAsync(
                pod.Id,
                PodLifecycleOperation.Replacement,
                ownerId,
                cancellationToken);
        }
    }

    private async Task<PodRecoveryResult> ReplacePodAsync(
        GpuPod pod,
        Guid organizationId,
        string source,
        Guid? userId,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        var oldProviderPodId = pod.ProviderPodId!;
        var modelsToRestore = await dbContext.AiModels
            .Where(m => m.OrganizationId == organizationId
                && m.PodId == pod.Id
                && m.Status != ModelStatus.Deleted)
            .Select(m => m.FullName)
            .ToListAsync(cancellationToken);

        var configuration = pod.Configuration
            ?? throw new InvalidOperationException("Pod configuration is missing.");

        var createOptions = PodCreateOptionsFactory.FromConfiguration(pod, configuration);
        PodInfo? createdPod = null;
        string? createdProviderPodId = null;

        try
        {
            logger.LogWarning(
                "Start failed for pod {PodId} ({ProviderPodId}). Creating replacement. Reason: {Reason}",
                pod.Id,
                oldProviderPodId,
                failureReason);

            createdPod = await podService.CreatePodAsync(pod.Provider, createOptions, cancellationToken);
            createdProviderPodId = createdPod.ProviderPodId;

            var runningPod = await EnsureReplacementRunningAsync(
                pod,
                createdPod,
                cancellationToken);

            if (runningPod is null)
            {
                return Failure("Replacement pod was created but could not be started.");
            }

            var now = dateTimeService.UtcNow;
            pod.ProviderPodId = runningPod.ProviderPodId;
            pod.LastStartedAt = now;
            pod.LastActivityAt = now;
            pod.UpdatedAt = now;
            pod.UpdatedBy = ResolveActorId(userId, pod).ToString();

            var syncedPod = await podService.SyncPodStatusAsync(pod, cancellationToken);
            pod.Status = syncedPod.Status;

            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = now,
                    Message = $"Pod replaced after start failure. Previous provider pod: {oldProviderPodId}.",
                },
                cancellationToken);

            await dbContext.AddPodLifecycleEventAsync(
                new PodLifecycleEvent
                {
                    PodId = pod.Id,
                    EventType = PodLifecycleEventType.PodReplaced,
                    Source = source,
                    UserId = userId,
                    Timestamp = now,
                    Message = failureReason is null
                        ? "Pod replaced with a new provider instance after start failure."
                        : $"Pod replaced after start failure: {failureReason}",
                },
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            await MigrateModelsAsync(organizationId, pod.Id, modelsToRestore, userId, pod, cancellationToken);

            var deleteResult = await podService.DeletePodAsync(
                pod.Provider,
                oldProviderPodId,
                cancellationToken);

            if (!deleteResult.Success)
            {
                logger.LogWarning(
                    "Replacement succeeded for pod {PodId}, but deleting old provider pod {OldProviderPodId} failed: {Error}",
                    pod.Id,
                    oldProviderPodId,
                    deleteResult.ErrorMessage);
            }

            logger.LogInformation(
                "Pod {PodId} replaced. Old provider pod {OldProviderPodId}, new provider pod {NewProviderPodId}",
                pod.Id,
                oldProviderPodId,
                runningPod.ProviderPodId);

            return new PodRecoveryResult
            {
                Success = true,
                Pod = runningPod,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Pod replacement failed for pod {PodId} after start failure on {ProviderPodId}",
                pod.Id,
                oldProviderPodId);

            if (!string.IsNullOrWhiteSpace(createdProviderPodId))
            {
                try
                {
                    await podService.DeletePodAsync(pod.Provider, createdProviderPodId, cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    logger.LogWarning(
                        cleanupEx,
                        "Failed to clean up replacement provider pod {ProviderPodId}",
                        createdProviderPodId);
                }
            }

            return Failure(ex.Message);
        }
    }

    private async Task<PodInfo?> EnsureReplacementRunningAsync(
        GpuPod pod,
        PodInfo createdPod,
        CancellationToken cancellationToken)
    {
        var podInfo = createdPod;

        if (podInfo.Status != PodStatus.Running)
        {
            var startResult = await podService.StartPodAsync(
                pod.Provider,
                createdPod.ProviderPodId,
                cancellationToken);

            if (!startResult.Success)
            {
                return null;
            }

            podInfo = startResult.Pod ?? createdPod;
            if (startResult.Status != default)
            {
                podInfo = CopyPodInfo(podInfo, status: startResult.Status);
            }
        }

        if (podInfo.Status == PodStatus.Running)
        {
            return podInfo;
        }

        for (var attempt = 0; attempt < ApplicationConstants.MaxWakeHealthCheckAttempts; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(ApplicationConstants.WakeHealthCheckInterval, cancellationToken);
            }

            var synced = await podService.GetProviderPodAsync(
                pod.Provider,
                createdPod.ProviderPodId,
                cancellationToken);

            if (synced.Status == PodStatus.Running)
            {
                return synced;
            }

            podInfo = synced;
        }

        return podInfo.Status == PodStatus.Running ? podInfo : null;
    }

    private static PodInfo CopyPodInfo(PodInfo source, PodStatus? status = null) =>
        new()
        {
            ProviderPodId = source.ProviderPodId,
            Name = source.Name,
            Status = status ?? source.Status,
            GpuId = source.GpuId,
            GpuType = source.GpuType,
            Region = source.Region,
            TemplateId = source.TemplateId,
            ImageName = source.ImageName,
            ContainerDiskGb = source.ContainerDiskGb,
            VolumeDiskGb = source.VolumeDiskGb,
            PublicIp = source.PublicIp,
            Endpoint = source.Endpoint,
            Endpoints = source.Endpoints,
            HourlyCost = source.HourlyCost,
            LastStartedAt = source.LastStartedAt,
            LastStoppedAt = source.LastStoppedAt,
            StatusMessage = source.StatusMessage,
        };

    private async Task MigrateModelsAsync(
        Guid organizationId,
        Guid podId,
        IReadOnlyList<string> modelsToRestore,
        Guid? userId,
        GpuPod pod,
        CancellationToken cancellationToken)
    {
        if (modelsToRestore.Count == 0)
        {
            return;
        }

        var actorId = ResolveActorId(userId, pod);

        using var scope = serviceScopeFactory.CreateScope();
        var modelService = scope.ServiceProvider.GetRequiredService<IModelService>();

        try
        {
            await modelService.RefreshModelsAsync(organizationId, podId, actorId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Model refresh failed during pod replacement for pod {PodId}", podId);
        }

        foreach (var modelReference in modelsToRestore)
        {
            var existing = await dbContext.AiModels
                .Where(m => m.OrganizationId == organizationId && m.PodId == podId)
                .FirstOrDefaultAsync(
                    m => m.FullName == modelReference && m.Status != ModelStatus.Deleted,
                    cancellationToken);

            if (existing?.Status == ModelStatus.Available)
            {
                continue;
            }

            try
            {
                await modelService.StartPullAsync(
                    organizationId,
                    podId,
                    modelReference,
                    actorId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to queue model {ModelReference} during pod replacement for pod {PodId}",
                    modelReference,
                    podId);
            }
        }
    }

    private static Guid ResolveActorId(Guid? userId, GpuPod pod)
    {
        if (userId.HasValue)
        {
            return userId.Value;
        }

        return Guid.TryParse(pod.CreatedBy, out var creatorId) ? creatorId : Guid.Empty;
    }

    private static PodRecoveryResult Failure(string message) =>
        new()
        {
            Success = false,
            ErrorMessage = message,
        };
}
