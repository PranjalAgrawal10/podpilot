using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically runs health checks and evaluates alerts.
/// </summary>
public sealed class MonitoringWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<MonitoringWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitoringWorker"/> class.
    /// </summary>
    public MonitoringWorker(IServiceScopeFactory serviceScopeFactory, ILogger<MonitoringWorker> logger)
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
                await MonitorAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Monitoring worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task MonitorAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var monitoringService = scope.ServiceProvider.GetRequiredService<IMonitoringService>();

        var organizationIds = await dbContext.Organizations
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        foreach (var organizationId in organizationIds)
        {
            try
            {
                await monitoringService.RunHealthChecksAsync(organizationId, cancellationToken);
                await monitoringService.EvaluateAlertsAsync(organizationId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Monitoring failed for organization {OrganizationId}",
                    organizationId);
            }
        }
    }
}
