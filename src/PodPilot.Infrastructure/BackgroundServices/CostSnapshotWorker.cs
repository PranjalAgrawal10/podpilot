using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically persists cost snapshots for organizations.
/// </summary>
public sealed class CostSnapshotWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<CostSnapshotWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CostSnapshotWorker"/> class.
    /// </summary>
    public CostSnapshotWorker(IServiceScopeFactory serviceScopeFactory, ILogger<CostSnapshotWorker> logger)
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
                await RecordSnapshotsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cost snapshot worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RecordSnapshotsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var costCalculator = scope.ServiceProvider.GetRequiredService<ICostCalculator>();

        var organizationIds = await dbContext.Organizations
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        foreach (var organizationId in organizationIds)
        {
            try
            {
                await costCalculator.CalculateAndPersistAsync(
                    organizationId,
                    MetricsPeriod.Hourly,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Cost snapshot failed for organization {OrganizationId}",
                    organizationId);
            }
        }
    }
}
