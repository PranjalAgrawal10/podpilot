using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically records capacity snapshots for orchestrated organizations.
/// </summary>
public sealed class CapacityPlannerWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<CapacityPlannerWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityPlannerWorker"/> class.
    /// </summary>
    public CapacityPlannerWorker(IServiceScopeFactory serviceScopeFactory, ILogger<CapacityPlannerWorker> logger)
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
                logger.LogError(ex, "Capacity planner worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RecordSnapshotsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var capacityPlanner = scope.ServiceProvider.GetRequiredService<ICapacityPlanner>();

        var organizationIds = await dbContext.PodPools
            .Where(p => p.IsActive)
            .Select(p => p.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var organizationId in organizationIds)
        {
            try
            {
                await capacityPlanner.RecordSnapshotAsync(organizationId, poolId: null, cancellationToken);

                var poolIds = await dbContext.PodPools
                    .Where(p => p.OrganizationId == organizationId && p.IsActive)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                foreach (var poolId in poolIds)
                {
                    await capacityPlanner.RecordSnapshotAsync(organizationId, poolId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Capacity snapshot failed for organization {OrganizationId}",
                    organizationId);
            }
        }
    }
}
