using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Times out queued and long-running scheduler requests.
/// </summary>
public sealed class SchedulerTimeoutWorker : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<SchedulerTimeoutWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerTimeoutWorker"/> class.
    /// </summary>
    public SchedulerTimeoutWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SchedulerTimeoutWorker> logger)
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
                await ProcessTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduler timeout worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task ProcessTimeoutsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IRequestScheduler>();
        var cutoff = DateTime.UtcNow - Application.Common.ApplicationConstants.SchedulerQueueTimeout;

        var timedOut = await dbContext.GatewayRequests
            .Where(r => r.Status == GatewayRequestStatus.Queued && r.CreatedAt < cutoff)
            .OrderBy(r => r.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var request in timedOut)
        {
            try
            {
                await scheduler.TimeoutAsync(request.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Timeout processing failed for request {RequestId}", request.Id);
            }
        }
    }
}
