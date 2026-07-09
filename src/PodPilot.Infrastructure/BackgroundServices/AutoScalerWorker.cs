using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically evaluates auto-scaling policies for all organizations.
/// </summary>
public sealed class AutoScalerWorker : BackgroundService
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<AutoScalerWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoScalerWorker"/> class.
    /// </summary>
    public AutoScalerWorker(IServiceScopeFactory serviceScopeFactory, ILogger<AutoScalerWorker> logger)
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
                await EvaluateAllOrganizationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Auto-scaler worker failed.");
            }

            await Task.Delay(DefaultInterval, stoppingToken);
        }
    }

    private async Task EvaluateAllOrganizationsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var autoScaler = scope.ServiceProvider.GetRequiredService<IAutoScaler>();

        var organizationIds = await dbContext.PodPools
            .Where(p => p.IsActive)
            .Select(p => p.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var organizationId in organizationIds)
        {
            try
            {
                var results = await autoScaler.EvaluateAsync(organizationId, cancellationToken);
                foreach (var result in results.Where(r => r.Success))
                {
                    logger.LogInformation(
                        "Auto-scaler {Direction} succeeded for pool {PoolId} in organization {OrganizationId}: {Reason}",
                        result.Direction,
                        result.PoolId,
                        organizationId,
                        result.Reason);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Auto-scaler evaluation failed for organization {OrganizationId}",
                    organizationId);
            }
        }
    }
}
