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

        var modelBreakdowns = await BuildModelCostBreakdownsAsync(
            organizationId,
            pods,
            periodHours,
            modelName,
            cancellationToken);

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
            ModelBreakdowns = modelBreakdowns,
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

    private async Task<IReadOnlyList<ModelCostBreakdown>> BuildModelCostBreakdownsAsync(
        Guid organizationId,
        IReadOnlyList<GpuPod> pods,
        decimal periodHours,
        string? modelNameFilter,
        CancellationToken cancellationToken)
    {
        if (pods.Count == 0)
        {
            return [];
        }

        var podIds = pods.Select(p => p.Id).ToList();
        var podCostById = pods.ToDictionary(p => p.Id, p => p.HourlyCost ?? 0m);

        var oneDayAgo = dateTimeService.UtcNow.AddDays(-1);
        var requestQuery = dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId
                && podIds.Contains(r.GpuPodId)
                && r.CreatedAt >= oneDayAgo
                && r.Model != null
                && r.Model != string.Empty);

        if (!string.IsNullOrWhiteSpace(modelNameFilter))
        {
            requestQuery = requestQuery.Where(r => r.Model == modelNameFilter);
        }

        var modelRequests = await requestQuery
            .GroupBy(r => new { r.GpuPodId, r.Model })
            .Select(g => new { g.Key.GpuPodId, ModelName = g.Key.Model!, Count = g.Count() })
            .ToListAsync(cancellationToken);

        if (modelRequests.Count > 0)
        {
            var allocations = new Dictionary<string, (decimal Hourly, int Requests)>(StringComparer.OrdinalIgnoreCase);

            foreach (var podGroup in modelRequests.GroupBy(r => r.GpuPodId))
            {
                var podHourly = podCostById.GetValueOrDefault(podGroup.Key);
                var totalRequests = podGroup.Sum(r => r.Count);
                if (totalRequests == 0 || podHourly == 0m)
                {
                    continue;
                }

                foreach (var entry in podGroup)
                {
                    var share = (decimal)entry.Count / totalRequests * podHourly;
                    if (allocations.TryGetValue(entry.ModelName, out var existing))
                    {
                        allocations[entry.ModelName] = (existing.Hourly + share, existing.Requests + entry.Count);
                    }
                    else
                    {
                        allocations[entry.ModelName] = (share, entry.Count);
                    }
                }
            }

            return allocations
                .OrderByDescending(a => a.Value.Hourly)
                .Select(a => new ModelCostBreakdown
                {
                    ModelName = a.Key,
                    HourlyCost = decimal.Round(a.Value.Hourly, 6, MidpointRounding.AwayFromZero),
                    PeriodCost = decimal.Round(a.Value.Hourly * periodHours, 6, MidpointRounding.AwayFromZero),
                    RequestCount = a.Value.Requests,
                })
                .ToList();
        }

        var modelsQuery = dbContext.AiModels
            .Where(m => m.OrganizationId == organizationId
                && podIds.Contains(m.PodId)
                && m.Status != ModelStatus.Deleted);

        if (!string.IsNullOrWhiteSpace(modelNameFilter))
        {
            modelsQuery = modelsQuery.Where(m =>
                m.Name == modelNameFilter
                || (m.Name + ":" + m.Tag) == modelNameFilter);
        }

        var installedModels = await modelsQuery.ToListAsync(cancellationToken);
        if (installedModels.Count == 0)
        {
            return [];
        }

        var equalSplit = new Dictionary<string, (decimal Hourly, int Count)>(StringComparer.OrdinalIgnoreCase);
        foreach (var podGroup in installedModels.GroupBy(m => m.PodId))
        {
            var podHourly = podCostById.GetValueOrDefault(podGroup.Key);
            var modelCount = podGroup.Count();
            if (modelCount == 0 || podHourly == 0m)
            {
                continue;
            }

            var share = podHourly / modelCount;
            foreach (var model in podGroup)
            {
                var name = model.FullName;
                if (equalSplit.TryGetValue(name, out var existing))
                {
                    equalSplit[name] = (existing.Hourly + share, existing.Count + 1);
                }
                else
                {
                    equalSplit[name] = (share, 1);
                }
            }
        }

        return equalSplit
            .OrderByDescending(a => a.Value.Hourly)
            .Select(a => new ModelCostBreakdown
            {
                ModelName = a.Key,
                HourlyCost = decimal.Round(a.Value.Hourly, 6, MidpointRounding.AwayFromZero),
                PeriodCost = decimal.Round(a.Value.Hourly * periodHours, 6, MidpointRounding.AwayFromZero),
                RequestCount = a.Value.Count,
            })
            .ToList();
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
