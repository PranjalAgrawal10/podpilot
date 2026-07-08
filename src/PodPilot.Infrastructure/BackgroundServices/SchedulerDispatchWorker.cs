using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Scheduler;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Dequeues and dispatches scheduled gateway requests.
/// </summary>
public sealed class SchedulerDispatchWorker : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<SchedulerDispatchWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerDispatchWorker"/> class.
    /// </summary>
    public SchedulerDispatchWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SchedulerDispatchWorker> logger)
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
                await ProcessQueuesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduler dispatch worker failed.");
            }

            await Task.Delay(Application.Common.ApplicationConstants.SchedulerWorkerInterval, stoppingToken);
        }
    }

    private async Task ProcessQueuesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var requestQueue = scope.ServiceProvider.GetRequiredService<IRequestQueue>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IRequestScheduler>();
        var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLockService>();

        var organizationIds = await dbContext.GatewayRequests
            .Where(r => r.Status == GatewayRequestStatus.Queued)
            .Select(r => r.OrganizationId)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync(cancellationToken);

        foreach (var organizationId in organizationIds)
        {
            await using var orgLock = await distributedLock.TryAcquireAsync(
                $"scheduler:org:{organizationId}",
                Application.Common.ApplicationConstants.SchedulerLockExpiry,
                cancellationToken);

            if (orgLock is null)
            {
                continue;
            }

            var item = await requestQueue.DequeueAsync(organizationId, cancellationToken);
            if (item is null)
            {
                continue;
            }

            try
            {
                await scheduler.ProcessQueuedItemAsync(item, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to dispatch queued request {RequestId}", item.RequestId);
            }
        }
    }
}
