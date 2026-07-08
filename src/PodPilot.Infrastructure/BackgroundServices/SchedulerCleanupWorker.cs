using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Cleans up stale queue entries and inactive request payloads.
/// </summary>
public sealed class SchedulerCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<SchedulerCleanupWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerCleanupWorker"/> class.
    /// </summary>
    public SchedulerCleanupWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SchedulerCleanupWorker> logger)
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
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduler cleanup worker failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var staleQueue = await dbContext.RequestQueueEntries
            .Where(e => !e.IsActive || e.EnqueuedAt < cutoff)
            .OrderBy(e => e.EnqueuedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        if (staleQueue.Count > 0)
        {
            foreach (var entry in staleQueue)
            {
                entry.IsActive = false;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Deactivated {Count} stale queue entries", staleQueue.Count);
        }
    }
}
