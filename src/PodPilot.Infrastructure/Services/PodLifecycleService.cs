using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Lifecycle;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Orchestrates automatic GPU pod wake and shutdown lifecycle operations.
/// </summary>
public sealed class PodLifecycleService : IPodLifecycleService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<PodLifecycleService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodLifecycleService"/> class.
    /// </summary>
    public PodLifecycleService(
        IApplicationDbContext dbContext,
        IPodService podService,
        IPodNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<PodLifecycleService> logger)
    {
        this.dbContext = dbContext;
        this.podService = podService;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task RecordActivityAsync(
        Guid podId,
        PodActivityType activityType,
        string source,
        Guid? userId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        await dbContext.AddPodActivityAsync(
            new PodActivity
            {
                PodId = podId,
                ActivityType = activityType,
                Timestamp = now,
                Source = source,
                UserId = userId,
                Metadata = metadata,
            },
            cancellationToken);

        await UpdateLastActivityAsync(podId, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateLastActivityAsync(
        Guid podId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var pod = await dbContext.GpuPods.FirstOrDefaultAsync(p => p.Id == podId, cancellationToken);
        if (pod is null)
        {
            return;
        }

        pod.LastActivityAt = timestamp;
        var policy = await dbContext.PodIdlePolicies.FirstOrDefaultAsync(p => p.PodId == podId, cancellationToken);
        if (policy is not null)
        {
            policy.IdleDetectedAt = null;
        }
    }

    /// <inheritdoc />
    public async Task<PodWakeResult> WakePodAsync(
        Guid podId,
        Guid organizationId,
        string source,
        Guid? userId = null,
        bool processImmediately = false,
        CancellationToken cancellationToken = default)
    {
        var pod = await LoadPodAsync(podId, organizationId, cancellationToken);
        var policy = await GetOrCreateIdlePolicyAsync(podId, cancellationToken);

        if (!policy.AutoWakeEnabled && source != "api")
        {
            return new PodWakeResult
            {
                Success = false,
                ErrorMessage = "Auto wake is disabled for this pod.",
            };
        }

        if (pod.Status == PodStatus.Running || pod.Status == PodStatus.Starting)
        {
            return new PodWakeResult
            {
                Success = true,
                Status = pod.Status.ToString(),
            };
        }

        if (pod.Status != PodStatus.Stopped && pod.Status != PodStatus.Unknown)
        {
            return new PodWakeResult
            {
                Success = false,
                ErrorMessage = $"Pod cannot be woken from status '{pod.Status}'.",
            };
        }

        var hasPending = await dbContext.PodWakeRequests.AnyAsync(
            r => r.PodId == podId
                && (r.Status == PodWakeRequestStatus.Pending || r.Status == PodWakeRequestStatus.Processing),
            cancellationToken);

        if (hasPending)
        {
            return new PodWakeResult
            {
                Success = false,
                ErrorMessage = "A wake request is already in progress for this pod.",
            };
        }

        var now = dateTimeService.UtcNow;
        var wakeRequest = new PodWakeRequest
        {
            PodId = podId,
            OrganizationId = organizationId,
            Status = PodWakeRequestStatus.Pending,
            Source = source,
            UserId = userId,
            RequestedAt = now,
        };

        await dbContext.AddPodWakeRequestAsync(wakeRequest, cancellationToken);
        await AddLifecycleEventAsync(
            podId,
            PodLifecycleEventType.WakeRequested,
            source,
            userId,
            "Wake requested.",
            cancellationToken);
        await RecordActivityInternalAsync(podId, PodActivityType.WakeRequested, source, userId, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Wake requested for pod {PodId} from source {Source}", podId, source);

        await notificationService.NotifyLifecycleEventAsync(
            organizationId,
            podId,
            "PodWaking",
            new { wakeRequestId = wakeRequest.Id },
            cancellationToken);

        if (processImmediately)
        {
            return await ProcessWakeRequestAsync(wakeRequest, cancellationToken);
        }

        return new PodWakeResult
        {
            Success = true,
            Queued = true,
            WakeRequestId = wakeRequest.Id,
            Status = PodStatus.Starting.ToString(),
        };
    }

    /// <inheritdoc />
    public async Task<PodWakeResult> ProcessWakeRequestAsync(
        PodWakeRequest request,
        CancellationToken cancellationToken = default)
    {
        var ownerId = $"wake-worker:{request.Id}";
        if (!await TryAcquireLockAsync(request.PodId, PodLifecycleOperation.Wake, ownerId, cancellationToken))
        {
            return new PodWakeResult
            {
                Success = false,
                ErrorMessage = "Wake operation is already in progress.",
            };
        }

        try
        {
            request.Status = PodWakeRequestStatus.Processing;
            request.ProcessingStartedAt = dateTimeService.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            var pod = await dbContext.GpuPods
                .Include(p => p.Provider)
                    .ThenInclude(pr => pr.Credential)
                .FirstAsync(p => p.Id == request.PodId, cancellationToken);

            logger.LogInformation("Wake started for pod {PodId}", request.PodId);

            await AddLifecycleEventAsync(
                request.PodId,
                PodLifecycleEventType.WakeStarted,
                request.Source,
                request.UserId,
                "Wake started.",
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await notificationService.NotifyLifecycleEventAsync(
                request.OrganizationId,
                request.PodId,
                "WakeStarted",
                null,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(pod.ProviderPodId))
            {
                throw new InvalidOperationException("Pod has not been provisioned on the provider.");
            }

            var startResult = await podService.StartPodAsync(
                pod.Provider,
                pod.ProviderPodId,
                cancellationToken);

            if (!startResult.Success)
            {
                throw new InvalidOperationException(startResult.ErrorMessage ?? "Provider start failed.");
            }

            if (startResult.Pod is not null)
            {
                podService.ApplyProviderStatus(pod, startResult.Pod, dateTimeService.UtcNow);
            }
            else
            {
                pod.Status = startResult.Status;
            }

            pod.LastStartedAt = dateTimeService.UtcNow;
            pod.LastActivityAt = dateTimeService.UtcNow;
            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = dateTimeService.UtcNow,
                    Message = "Pod wake started.",
                },
                cancellationToken);

            if (pod.Status != PodStatus.Running)
            {
                for (var attempt = 0; attempt < ApplicationConstants.MaxWakeHealthCheckAttempts; attempt++)
                {
                    if (attempt > 0)
                    {
                        await Task.Delay(ApplicationConstants.WakeHealthCheckInterval, cancellationToken);
                    }

                    var info = await podService.SyncPodStatusAsync(pod, cancellationToken);
                    if (info.Status == PodStatus.Running)
                    {
                        break;
                    }
                }
            }

            request.Status = PodWakeRequestStatus.Completed;
            request.CompletedAt = dateTimeService.UtcNow;

            await AddLifecycleEventAsync(
                request.PodId,
                PodLifecycleEventType.WakeCompleted,
                request.Source,
                request.UserId,
                "Wake completed.",
                cancellationToken);
            await AddLifecycleEventAsync(
                request.PodId,
                PodLifecycleEventType.PodStarted,
                request.Source,
                request.UserId,
                "Pod is running.",
                cancellationToken);
            await RecordActivityInternalAsync(
                request.PodId,
                PodActivityType.Started,
                request.Source,
                request.UserId,
                dateTimeService.UtcNow,
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Wake completed for pod {PodId}", request.PodId);

            await notificationService.NotifyPodStatusChangedAsync(
                request.OrganizationId,
                request.PodId,
                pod.Status.ToString(),
                cancellationToken);
            await notificationService.NotifyLifecycleEventAsync(
                request.OrganizationId,
                request.PodId,
                "WakeCompleted",
                null,
                cancellationToken);
            await notificationService.NotifyLifecycleEventAsync(
                request.OrganizationId,
                request.PodId,
                "PodStarted",
                null,
                cancellationToken);

            return new PodWakeResult
            {
                Success = true,
                WakeRequestId = request.Id,
                Status = pod.Status.ToString(),
            };
        }
        catch (Exception ex)
        {
            request.Status = PodWakeRequestStatus.Failed;
            request.CompletedAt = dateTimeService.UtcNow;
            request.ErrorMessage = ex.Message;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning(ex, "Wake failed for pod {PodId}", request.PodId);

            return new PodWakeResult
            {
                Success = false,
                WakeRequestId = request.Id,
                ErrorMessage = ex.Message,
            };
        }
        finally
        {
            await ReleaseLockAsync(request.PodId, PodLifecycleOperation.Wake, ownerId, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<PodShutdownResult> ShutdownPodAsync(
        Guid podId,
        Guid organizationId,
        string source,
        string reason,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var ownerId = $"shutdown:{Guid.NewGuid():N}";
        if (!await TryAcquireLockAsync(podId, PodLifecycleOperation.Shutdown, ownerId, cancellationToken))
        {
            return new PodShutdownResult
            {
                Success = false,
                ErrorMessage = "Shutdown operation is already in progress.",
            };
        }

        try
        {
            var pod = await LoadPodAsync(podId, organizationId, cancellationToken);
            var policy = await GetOrCreateIdlePolicyAsync(podId, cancellationToken);

            if (pod.Status != PodStatus.Running && pod.Status != PodStatus.Starting)
            {
                return new PodShutdownResult
                {
                    Success = false,
                    ErrorMessage = $"Pod cannot be shut down from status '{pod.Status}'.",
                };
            }

            var now = dateTimeService.UtcNow;
            var runningReference = pod.LastStartedAt ?? pod.CreatedAt;
            var runningMinutes = (now - runningReference).TotalMinutes;
            if (source == "idle-worker" && runningMinutes < policy.MinimumRunningTimeMinutes)
            {
                return new PodShutdownResult
                {
                    Success = false,
                    ErrorMessage = "Minimum running time has not elapsed.",
                };
            }

            logger.LogInformation("Shutdown requested for pod {PodId}: {Reason}", podId, reason);

            await AddLifecycleEventAsync(
                podId,
                PodLifecycleEventType.ShutdownRequested,
                source,
                userId,
                reason,
                cancellationToken);
            await RecordActivityInternalAsync(
                podId,
                PodActivityType.ShutdownRequested,
                source,
                userId,
                now,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await notificationService.NotifyLifecycleEventAsync(
                organizationId,
                podId,
                "ShutdownRequested",
                new { reason },
                cancellationToken);

            if (string.IsNullOrWhiteSpace(pod.ProviderPodId))
            {
                throw new InvalidOperationException("Pod has not been provisioned on the provider.");
            }

            await AddLifecycleEventAsync(
                podId,
                PodLifecycleEventType.ShutdownStarted,
                source,
                userId,
                "Shutdown started.",
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var stopResult = await podService.StopPodAsync(
                pod.Provider,
                pod.ProviderPodId,
                cancellationToken);

            if (!stopResult.Success)
            {
                throw new InvalidOperationException(stopResult.ErrorMessage ?? "Provider stop failed.");
            }

            if (stopResult.Pod is not null)
            {
                podService.ApplyProviderStatus(pod, stopResult.Pod, now);
            }
            else
            {
                pod.Status = stopResult.Status;
            }

            pod.LastStoppedAt = now;
            policy.IdleDetectedAt = null;

            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = now,
                    Message = reason,
                },
                cancellationToken);

            await AddLifecycleEventAsync(
                podId,
                PodLifecycleEventType.ShutdownCompleted,
                source,
                userId,
                "Shutdown completed.",
                cancellationToken);
            await AddLifecycleEventAsync(
                podId,
                PodLifecycleEventType.PodStopped,
                source,
                userId,
                "Pod stopped.",
                cancellationToken);
            await AddLifecycleEventAsync(
                podId,
                PodLifecycleEventType.PodSleeping,
                source,
                userId,
                "Pod is sleeping.",
                cancellationToken);
            await RecordActivityInternalAsync(
                podId,
                PodActivityType.Stopped,
                source,
                userId,
                now,
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Shutdown completed for pod {PodId}", podId);

            await notificationService.NotifyPodStatusChangedAsync(
                organizationId,
                podId,
                pod.Status.ToString(),
                cancellationToken);
            await notificationService.NotifyLifecycleEventAsync(
                organizationId,
                podId,
                "ShutdownCompleted",
                null,
                cancellationToken);
            await notificationService.NotifyLifecycleEventAsync(
                organizationId,
                podId,
                "PodStopped",
                null,
                cancellationToken);
            await notificationService.NotifyLifecycleEventAsync(
                organizationId,
                podId,
                "PodSleeping",
                null,
                cancellationToken);

            return new PodShutdownResult
            {
                Success = true,
                Status = pod.Status.ToString(),
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Shutdown failed for pod {PodId}", podId);
            return new PodShutdownResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
        finally
        {
            await ReleaseLockAsync(podId, PodLifecycleOperation.Shutdown, ownerId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GpuPod>> GetRunningPodsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.GpuPods
            .Include(p => p.IdlePolicy)
            .Where(p => p.Status == PodStatus.Running && p.ProviderPodId != null)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GpuPod>> GetIdlePodsAsync(CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var runningPods = await GetRunningPodsAsync(cancellationToken);
        var idlePods = new List<GpuPod>();

        foreach (var pod in runningPods)
        {
            var policy = pod.IdlePolicy ?? await GetOrCreateIdlePolicyAsync(pod.Id, cancellationToken);
            if (!policy.AutoShutdownEnabled)
            {
                continue;
            }

            var idleStatus = EvaluateIdleStatus(pod, policy, now);
            if (idleStatus.IsIdle && idleStatus.NextShutdownAt <= now)
            {
                idlePods.Add(pod);
            }
        }

        return idlePods;
    }

    /// <inheritdoc />
    public PodIdleStatus EvaluateIdleStatus(GpuPod pod, PodIdlePolicy policy, DateTime utcNow)
    {
        var lastActivity = pod.LastActivityAt ?? pod.LastStartedAt ?? pod.CreatedAt;
        var idleMinutes = Math.Max(0, (utcNow - lastActivity).TotalMinutes);
        var isPastTimeout = idleMinutes >= policy.IdleTimeoutMinutes;

        DateTime? idleDetectedAt = policy.IdleDetectedAt;
        DateTime? nextShutdownAt = null;
        var isIdle = false;

        if (pod.Status == PodStatus.Running && isPastTimeout)
        {
            isIdle = true;
            idleDetectedAt ??= lastActivity.AddMinutes(policy.IdleTimeoutMinutes);
            nextShutdownAt = idleDetectedAt.Value.AddMinutes(policy.GracePeriodMinutes);
        }

        return new PodIdleStatus
        {
            IsIdle = isIdle,
            IdleMinutes = idleMinutes,
            IdleDetectedAt = idleDetectedAt,
            NextShutdownAt = nextShutdownAt,
            LastActivityAt = lastActivity,
        };
    }

    /// <inheritdoc />
    public async Task<PodIdlePolicy> GetOrCreateIdlePolicyAsync(
        Guid podId,
        CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.PodIdlePolicies.FirstOrDefaultAsync(p => p.PodId == podId, cancellationToken);
        if (policy is not null)
        {
            return policy;
        }

        var now = dateTimeService.UtcNow;
        policy = new PodIdlePolicy
        {
            PodId = podId,
            IdleTimeoutMinutes = ApplicationConstants.DefaultIdleTimeoutMinutes,
            GracePeriodMinutes = ApplicationConstants.DefaultGracePeriodMinutes,
            AutoShutdownEnabled = true,
            AutoWakeEnabled = true,
            MinimumRunningTimeMinutes = ApplicationConstants.DefaultMinimumRunningTimeMinutes,
            CreatedAt = now,
        };

        await dbContext.AddPodIdlePolicyAsync(policy, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return policy;
    }

    /// <inheritdoc />
    public async Task<bool> TryAcquireLockAsync(
        Guid podId,
        PodLifecycleOperation operation,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var expiresAt = now.AddSeconds(ApplicationConstants.LifecycleLockDurationSeconds);

        var existing = await dbContext.PodLifecycleLocks
            .FirstOrDefaultAsync(
                l => l.PodId == podId && l.Operation == operation,
                cancellationToken);

        if (existing is not null)
        {
            if (existing.ExpiresAt > now && existing.OwnerId != ownerId)
            {
                return false;
            }

            existing.OwnerId = ownerId;
            existing.AcquiredAt = now;
            existing.ExpiresAt = expiresAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        try
        {
            await dbContext.AddPodLifecycleLockAsync(
                new PodLifecycleLock
                {
                    PodId = podId,
                    Operation = operation,
                    OwnerId = ownerId,
                    AcquiredAt = now,
                    ExpiresAt = expiresAt,
                },
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task ReleaseLockAsync(
        Guid podId,
        PodLifecycleOperation operation,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.PodLifecycleLocks
            .FirstOrDefaultAsync(
                l => l.PodId == podId && l.Operation == operation && l.OwnerId == ownerId,
                cancellationToken);

        if (existing is not null)
        {
            await dbContext.RemovePodLifecycleLockAsync(existing, cancellationToken);
        }
    }

    private async Task<GpuPod> LoadPodAsync(
        Guid podId,
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.GpuPods
            .Include(p => p.Provider)
                .ThenInclude(pr => pr.Credential)
            .FirstAsync(p => p.Id == podId && p.OrganizationId == organizationId, cancellationToken);

    private async Task AddLifecycleEventAsync(
        Guid podId,
        PodLifecycleEventType eventType,
        string source,
        Guid? userId,
        string? message,
        CancellationToken cancellationToken)
    {
        await dbContext.AddPodLifecycleEventAsync(
            new PodLifecycleEvent
            {
                PodId = podId,
                EventType = eventType,
                Timestamp = dateTimeService.UtcNow,
                Source = source,
                UserId = userId,
                Message = message,
            },
            cancellationToken);
    }

    private async Task RecordActivityInternalAsync(
        Guid podId,
        PodActivityType activityType,
        string source,
        Guid? userId,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        await dbContext.AddPodActivityAsync(
            new PodActivity
            {
                PodId = podId,
                ActivityType = activityType,
                Timestamp = timestamp,
                Source = source,
                UserId = userId,
            },
            cancellationToken);
        await UpdateLastActivityAsync(podId, timestamp, cancellationToken);
    }
}
