using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Provides usage analytics from gateway requests and persisted statistics.
/// </summary>
public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<AnalyticsService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsService"/> class.
    /// </summary>
    public AnalyticsService(
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<AnalyticsService> logger)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<AnalyticsSummary> GetAnalyticsAsync(
        Guid organizationId,
        MetricsPeriod period,
        DateTime? from = null,
        DateTime? to = null,
        Guid? providerId = null,
        Guid? podId = null,
        string? modelName = null,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var periodStart = from ?? GetPeriodStart(now, period);
        var periodEnd = to ?? now;

        var requestsQuery = dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt >= periodStart && r.CreatedAt <= periodEnd);

        if (podId.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.GpuPodId == podId.Value);
        }

        if (!string.IsNullOrWhiteSpace(modelName))
        {
            requestsQuery = requestsQuery.Where(r => r.Model == modelName);
        }

        if (providerId.HasValue)
        {
            var providerPodIds = await dbContext.GpuPods
                .Where(p => p.OrganizationId == organizationId && p.ProviderId == providerId.Value)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            requestsQuery = requestsQuery.Where(r => providerPodIds.Contains(r.GpuPodId));
        }

        var requests = await requestsQuery
            .Include(r => r.Latency)
            .ToListAsync(cancellationToken);

        var completed = requests.Where(r => r.Status == GatewayRequestStatus.Completed).ToList();
        var errors = requests.Count(r =>
            r.Status == GatewayRequestStatus.Failed || r.Status == GatewayRequestStatus.TimedOut);

        var totalLatency = completed
            .Where(r => r.Latency is not null)
            .Sum(r => (long)r.Latency!.TotalLatencyMs);

        var averageLatency = completed.Count == 0
            ? 0
            : completed
                .Where(r => r.Latency is not null)
                .Select(r => (double)r.Latency!.TotalLatencyMs)
                .DefaultIfEmpty(0)
                .Average();

        var modelBreakdowns = requests
            .Where(r => !string.IsNullOrWhiteSpace(r.Model))
            .GroupBy(r => r.Model!)
            .Select(g => new ModelUsageBreakdown
            {
                ModelName = g.Key,
                RequestCount = g.Count(),
                TokenCount = g.Count(r => r.Status == GatewayRequestStatus.Completed) * 500L,
                AverageLatencyMs = g
                    .Where(r => r.Latency is not null)
                    .Select(r => (double)r.Latency!.TotalLatencyMs)
                    .DefaultIfEmpty(0)
                    .Average(),
            })
            .OrderByDescending(b => b.RequestCount)
            .ToList();

        var pods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId)
            .Include(p => p.Provider)
            .ToListAsync(cancellationToken);

        var providerBreakdowns = requests
            .Join(pods, r => r.GpuPodId, p => p.Id, (r, p) => new { Request = r, Pod = p })
            .GroupBy(x => new { x.Pod.ProviderId, ProviderName = x.Pod.Provider?.Name ?? string.Empty })
            .Select(g => new ProviderUsageBreakdown
            {
                ProviderId = g.Key.ProviderId,
                ProviderName = g.Key.ProviderName,
                RequestCount = g.Count(),
                InferenceCount = g.Count(x => x.Request.Status == GatewayRequestStatus.Completed),
            })
            .OrderByDescending(b => b.RequestCount)
            .ToList();

        var podBreakdowns = requests
            .GroupBy(r => r.GpuPodId)
            .Select(g =>
            {
                var pod = pods.FirstOrDefault(p => p.Id == g.Key);
                var uptime = 0L;
                if (pod?.LastStartedAt is not null)
                {
                    var effectiveStart = pod.LastStartedAt.Value > periodStart
                        ? pod.LastStartedAt.Value
                        : periodStart;
                    uptime = (long)Math.Max(0, (periodEnd - effectiveStart).TotalSeconds);
                }

                return new PodUsageBreakdown
                {
                    PodId = g.Key,
                    PodName = pod?.Name ?? string.Empty,
                    RequestCount = g.Count(),
                    UptimeSeconds = uptime,
                };
            })
            .OrderByDescending(b => b.RequestCount)
            .ToList();

        var persistedStats = await dbContext.UsageStatistics
            .Where(s => s.OrganizationId == organizationId && s.RecordedAt >= periodStart && s.RecordedAt <= periodEnd)
            .ToListAsync(cancellationToken);

        var totalUptime = persistedStats.Count > 0
            ? persistedStats.Max(s => s.UptimeSeconds)
            : podBreakdowns.Sum(b => b.UptimeSeconds);

        logger.LogDebug(
            "Analytics for organization {OrganizationId}: {RequestCount} requests in period {Period}",
            organizationId,
            requests.Count,
            period);

        return new AnalyticsSummary
        {
            Period = period,
            TotalRequests = requests.Count,
            TotalTokens = completed.Count * 500L,
            TotalInferences = completed.Count,
            AverageLatencyMs = averageLatency,
            ErrorRate = requests.Count == 0 ? 0 : (double)errors / requests.Count,
            TotalUptimeSeconds = totalUptime,
            ModelBreakdowns = modelBreakdowns,
            ProviderBreakdowns = providerBreakdowns,
            PodBreakdowns = podBreakdowns,
        };
    }

    private static DateTime GetPeriodStart(DateTime now, MetricsPeriod period) =>
        period switch
        {
            MetricsPeriod.Hourly => now.AddHours(-1),
            MetricsPeriod.Daily => now.AddDays(-1),
            MetricsPeriod.Weekly => now.AddDays(-7),
            MetricsPeriod.Monthly => now.AddDays(-30),
            _ => now.AddDays(-1),
        };
}
