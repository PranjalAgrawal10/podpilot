using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically collects and persists metrics and usage statistics.
/// </summary>
public sealed class MetricsCollectionWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<MetricsCollectionWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollectionWorker"/> class.
    /// </summary>
    public MetricsCollectionWorker(IServiceScopeFactory serviceScopeFactory, ILogger<MetricsCollectionWorker> logger)
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
                await CollectAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Metrics collection worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CollectAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var metricsCollector = scope.ServiceProvider.GetRequiredService<IMetricsCollector>();

        var organizationIds = await dbContext.Organizations
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        foreach (var organizationId in organizationIds)
        {
            try
            {
                await metricsCollector.CollectAndPersistAsync(organizationId, cancellationToken);
                await metricsCollector.CollectUsageAndPersistAsync(
                    organizationId,
                    new MetricsPeriodFilter { Period = MetricsPeriod.Hourly },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Metrics collection failed for organization {OrganizationId}",
                    organizationId);
            }
        }
    }
}
