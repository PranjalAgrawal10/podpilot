using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Calculates cost snapshots from pod hourly rates and lifecycle savings.
/// </summary>
public sealed class CostCalculatorService : ICostCalculator
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly IObservabilityNotificationService notificationService;
    private readonly ILogger<CostCalculatorService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CostCalculatorService"/> class.
    /// </summary>
    public CostCalculatorService(
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        IObservabilityNotificationService notificationService,
        ILogger<CostCalculatorService> logger)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<CostSummary> CalculateAsync(
        Guid organizationId,
        MetricsPeriod period,
        Guid? providerId = null,
        Guid? podId = null,
        string? modelName = null,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var podsQuery = dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId
                && (p.Status == PodStatus.Running
                    || p.Status == PodStatus.Starting
                    || p.Status == PodStatus.Restarting));

        if (providerId.HasValue)
        {
            podsQuery = podsQuery.Where(p => p.ProviderId == providerId.Value);
        }

        if (podId.HasValue)
        {
            podsQuery = podsQuery.Where(p => p.Id == podId.Value);
        }

        var pods = await podsQuery
            .Include(p => p.Provider)
            .ToListAsync(cancellationToken);

        var hourlyCost = pods.Sum(p => p.HourlyCost ?? 0m);
        var periodHours = GetPeriodHours(period);
        var periodCost = hourlyCost * periodHours;

        var podBreakdowns = pods.Select(p => new PodCostBreakdown
        {
            PodId = p.Id,
            PodName = p.Name,
            HourlyCost = p.HourlyCost ?? 0m,
            PeriodCost = (p.HourlyCost ?? 0m) * periodHours,
        }).ToList();

        var providerBreakdowns = pods
            .GroupBy(p => p.ProviderId)
            .Select(g => new ProviderCostBreakdown
            {
                ProviderId = g.Key,
                ProviderName = g.First().Provider?.Name ?? string.Empty,
                HourlyCost = g.Sum(p => p.HourlyCost ?? 0m),
                PeriodCost = g.Sum(p => (p.HourlyCost ?? 0m) * periodHours),
            })
            .ToList();

        var savingsStart = now.AddDays(-30);
        var shutdownEvents = await dbContext.PodLifecycleEvents
            .Where(e =>
                e.Pod.OrganizationId == organizationId
                && e.EventType == PodLifecycleEventType.ShutdownCompleted
                && e.Timestamp >= savingsStart)
            .Include(e => e.Pod)
            .ToListAsync(cancellationToken);

        var autoShutdownSavings = shutdownEvents.Sum(e =>
        {
            var hourly = e.Pod.HourlyCost ?? 0m;
            return hourly * 2m;
        });

        return new CostSummary
        {
            Period = period,
            CalculatedAt = now,
            HourlyCost = hourlyCost,
            DailyCost = hourlyCost * 24m,
            WeeklyCost = hourlyCost * 24m * 7m,
            MonthlyCost = hourlyCost * 24m * 30m,
            ProjectedMonthlyCost = hourlyCost * 24m * 30m,
            AutoShutdownSavings = autoShutdownSavings,
            PodBreakdowns = podBreakdowns,
            ProviderBreakdowns = providerBreakdowns,
        };
    }

    /// <inheritdoc />
    public async Task<CostSummary> CalculateAndPersistAsync(
        Guid organizationId,
        MetricsPeriod period,
        CancellationToken cancellationToken = default)
    {
        var summary = await CalculateAsync(organizationId, period, cancellationToken: cancellationToken);

        var snapshot = new CostSnapshot
        {
            OrganizationId = organizationId,
            RecordedAt = summary.CalculatedAt,
            Period = period,
            HourlyCost = summary.HourlyCost,
            DailyCost = summary.DailyCost,
            WeeklyCost = summary.WeeklyCost,
            MonthlyCost = summary.MonthlyCost,
            ProjectedMonthlyCost = summary.ProjectedMonthlyCost,
            AutoShutdownSavings = summary.AutoShutdownSavings,
        };

        await dbContext.AddCostSnapshotAsync(snapshot, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyCostUpdatedAsync(organizationId, cancellationToken);

        logger.LogDebug(
            "Persisted cost snapshot for organization {OrganizationId}: Hourly {HourlyCost}",
            organizationId,
            summary.HourlyCost);

        return summary;
    }

    private static decimal GetPeriodHours(MetricsPeriod period) =>
        period switch
        {
            MetricsPeriod.Hourly => 1m,
            MetricsPeriod.Daily => 24m,
            MetricsPeriod.Weekly => 24m * 7m,
            MetricsPeriod.Monthly => 24m * 30m,
            _ => 1m,
        };
}
