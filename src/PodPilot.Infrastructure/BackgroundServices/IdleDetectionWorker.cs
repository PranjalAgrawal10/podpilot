using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Detects idle running pods and queues automatic shutdowns.
/// </summary>
public sealed class IdleDetectionWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<IdleDetectionWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdleDetectionWorker"/> class.
    /// </summary>
    public IdleDetectionWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<IdleDetectionWorker> logger)
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
                await DetectIdlePodsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Idle detection worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task DetectIdlePodsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var lifecycleService = scope.ServiceProvider.GetRequiredService<IPodLifecycleService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<IPodNotificationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();

        var now = dateTimeService.UtcNow;
        var runningPods = await lifecycleService.GetRunningPodsAsync(cancellationToken);

        foreach (var pod in runningPods)
        {
            try
            {
                var policy = pod.IdlePolicy ?? await lifecycleService.GetOrCreateIdlePolicyAsync(pod.Id, cancellationToken);
                if (!policy.AutoShutdownEnabled)
                {
                    continue;
                }

                var idleStatus = lifecycleService.EvaluateIdleStatus(pod, policy, now);

                if (!idleStatus.IsIdle)
                {
                    if (policy.IdleDetectedAt is not null)
                    {
                        policy.IdleDetectedAt = null;
                    }

                    continue;
                }

                if (policy.IdleDetectedAt is null)
                {
                    policy.IdleDetectedAt = idleStatus.IdleDetectedAt;
                    await dbContext.AddPodLifecycleEventAsync(
                        new Domain.Entities.PodLifecycleEvent
                        {
                            PodId = pod.Id,
                            EventType = PodLifecycleEventType.IdleDetected,
                            Timestamp = now,
                            Source = "idle-worker",
                            Message = $"Pod idle for {idleStatus.IdleMinutes:F0} minutes.",
                        },
                        cancellationToken);

                    logger.LogInformation(
                        "Idle detected for pod {PodId} after {IdleMinutes} minutes",
                        pod.Id,
                        idleStatus.IdleMinutes);

                    await notificationService.NotifyLifecycleEventAsync(
                        pod.OrganizationId,
                        pod.Id,
                        "IdleDetected",
                        new { idleStatus.IdleMinutes, idleStatus.NextShutdownAt },
                        cancellationToken);
                }

                if (idleStatus.NextShutdownAt is null || idleStatus.NextShutdownAt > now)
                {
                    continue;
                }

                var result = await lifecycleService.ShutdownPodAsync(
                    pod.Id,
                    pod.OrganizationId,
                    "idle-worker",
                    "Automatic shutdown after idle timeout and grace period.",
                    cancellationToken: cancellationToken);

                if (!result.Success)
                {
                    logger.LogWarning(
                        "Automatic shutdown failed for pod {PodId}: {Error}",
                        pod.Id,
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Idle detection failed for pod {PodId}", pod.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
