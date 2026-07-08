using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically synchronizes pod status with compute providers.
/// </summary>
public sealed class PodStatusSyncWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<PodStatusSyncWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodStatusSyncWorker"/> class.
    /// </summary>
    public PodStatusSyncWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PodStatusSyncWorker> logger)
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
                await SyncPodsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pod status sync worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SyncPodsAsync(CancellationToken cancellationToken)
    {
        await ImportProviderPodsAsync(cancellationToken);

        IReadOnlyList<Guid> podIds;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            podIds = await dbContext.GpuPods
                .Where(p => p.ProviderPodId != null
                    && p.Status != PodStatus.Deleted
                    && p.Status != PodStatus.Deleting
                    && p.Status != PodStatus.Failed)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        foreach (var podId in podIds)
        {
            await SyncSinglePodAsync(podId, cancellationToken);
        }
    }

    private async Task ImportProviderPodsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var podService = scope.ServiceProvider.GetRequiredService<IPodService>();

        var providers = await dbContext.ComputeProviders
            .Include(p => p.Credential)
            .Where(p => p.IsEnabled && p.IsValidated)
            .ToListAsync(cancellationToken);

        foreach (var provider in providers)
        {
            try
            {
                await podService.ImportProviderPodsAsync(provider, provider.OrganizationId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Pod import failed for provider {ProviderId}", provider.Id);
            }
        }
    }

    private async Task SyncSinglePodAsync(Guid podId, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var podService = scope.ServiceProvider.GetRequiredService<IPodService>();
        var podNotificationService = scope.ServiceProvider.GetRequiredService<IPodNotificationService>();
        var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();

        var utcNow = dateTimeService.UtcNow;
        var hasActiveLifecycleLock = await dbContext.PodLifecycleLocks
            .AnyAsync(
                l => l.PodId == podId && l.ExpiresAt > utcNow,
                cancellationToken);

        if (hasActiveLifecycleLock)
        {
            logger.LogDebug("Skipping pod status sync for {PodId} because a lifecycle lock is active", podId);
            return;
        }

        var pod = await dbContext.GpuPods
            .Include(p => p.Provider)
                .ThenInclude(provider => provider.Credential)
            .Include(p => p.Endpoints)
            .FirstOrDefaultAsync(p => p.Id == podId, cancellationToken);

        if (pod is null)
        {
            return;
        }

        try
        {
            var previousStatus = pod.Status;
            await podService.SyncPodStatusAsync(pod, cancellationToken);

            if (previousStatus != pod.Status)
            {
                await dbContext.AddPodStatusHistoryAsync(
                    new PodStatusHistory
                    {
                        GpuPodId = pod.Id,
                        Status = pod.Status,
                        RecordedAt = utcNow,
                        Message = "Status synchronized by background worker.",
                    },
                    cancellationToken);

                await podNotificationService.NotifyPodStatusChangedAsync(
                    pod.OrganizationId,
                    pod.Id,
                    pod.Status.ToString(),
                    cancellationToken);
            }

            pod.UpdatedAt = utcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning(
                "Pod status sync skipped for pod {PodId} due to a concurrent update",
                podId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Pod status sync failed for pod {PodId}", podId);
        }
    }
}
