using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Retries failed scheduler requests with exponential backoff.
/// </summary>
public sealed class SchedulerRetryWorker : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<SchedulerRetryWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerRetryWorker"/> class.
    /// </summary>
    public SchedulerRetryWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SchedulerRetryWorker> logger)
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
                await ProcessRetriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduler retry worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessRetriesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IRequestScheduler>();

        var failed = await dbContext.GatewayRequests
            .Where(r => r.Status == GatewayRequestStatus.Failed
                && r.RetryCount < ApplicationConstants.SchedulerMaxRetryAttempts
                && r.CompletedAt != null
                && r.CompletedAt < DateTime.UtcNow.AddMinutes(-1))
            .OrderBy(r => r.CompletedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var request in failed)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, request.RetryCount) * ApplicationConstants.SchedulerRetryBaseDelay.TotalSeconds);
            if (request.CompletedAt!.Value.Add(delay) > DateTime.UtcNow)
            {
                continue;
            }

            try
            {
                await scheduler.RetryAsync(request.Id, request.OrganizationId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Retry failed for request {RequestId}", request.Id);
            }
        }
    }
}
