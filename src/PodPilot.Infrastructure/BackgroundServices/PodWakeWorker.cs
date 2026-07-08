using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Processes queued pod wake requests.
/// </summary>
public sealed class PodWakeWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<PodWakeWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodWakeWorker"/> class.
    /// </summary>
    public PodWakeWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PodWakeWorker> logger)
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
                await ProcessWakeRequestsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pod wake worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessWakeRequestsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var lifecycleService = scope.ServiceProvider.GetRequiredService<IPodLifecycleService>();

        var pendingRequests = await dbContext.PodWakeRequests
            .Where(r => r.Status == PodWakeRequestStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var request in pendingRequests)
        {
            try
            {
                await lifecycleService.ProcessWakeRequestAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Wake request processing failed for pod {PodId}", request.PodId);
            }
        }
    }
}
