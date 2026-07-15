using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Predicts latency using history, health, and pod load signals.
/// </summary>
public sealed class LatencyPredictor : ILatencyPredictor
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LatencyPredictor"/> class.
    /// </summary>
    public LatencyPredictor(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<LatencyPrediction> PredictAsync(
        Guid organizationId,
        Guid providerId,
        string? modelName,
        CancellationToken cancellationToken = default)
    {
        var recent = await dbContext.LatencyHistories
            .AsNoTracking()
            .Where(h =>
                h.OrganizationId == organizationId &&
                h.AiProviderId == providerId &&
                (modelName == null || h.ModelName == modelName))
            .OrderByDescending(h => h.RecordedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var health = await dbContext.AiProviderHealthSnapshots
            .AsNoTracking()
            .Where(h => h.AiProviderId == providerId)
            .OrderByDescending(h => h.LastCheckedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var queueDepth = await dbContext.RequestQueueEntries
            .AsNoTracking()
            .CountAsync(q => q.OrganizationId == organizationId && q.IsActive, cancellationToken);

        var warmPods = await dbContext.GpuPods
            .AsNoTracking()
            .CountAsync(
                p => p.OrganizationId == organizationId && p.Status == PodStatus.Running,
                cancellationToken);

        var avgHistory = recent.Count > 0
            ? (int)recent.Average(r => r.LatencyMs)
            : health?.LatencyMs ?? 800;

        var podLoad = recent.Count > 0
            ? recent.Average(r => r.PodLoadPercent)
            : Math.Min(100, queueDepth * 5.0);

        var coldStartMs = warmPods > 0 ? 0 : 2500;
        if (recent.Any(r => r.WasColdStart))
        {
            coldStartMs = (int)recent.Where(r => r.WasColdStart).Average(r => r.ColdStartMs ?? 2500);
        }

        var queuePenalty = queueDepth * 40;
        var loadPenalty = (int)(podLoad * 3);
        var predicted = avgHistory + queuePenalty + loadPenalty + coldStartMs;

        return new LatencyPrediction
        {
            AverageResponseMs = avgHistory,
            QueueDepth = queueDepth,
            ProviderHealthLatencyMs = health?.LatencyMs,
            PodLoadPercent = podLoad,
            WarmPods = warmPods,
            ColdStartMs = coldStartMs,
            PredictedLatencyMs = Math.Clamp(predicted, 50, 120_000),
        };
    }
}
