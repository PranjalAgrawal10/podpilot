using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Estimates token and provider costs for routing candidates.
/// </summary>
public sealed class CostEstimator : ICostEstimator
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly IProviderCostRateCatalog costRateCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="CostEstimator"/> class.
    /// </summary>
    public CostEstimator(
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        IProviderCostRateCatalog costRateCatalog)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.costRateCatalog = costRateCatalog;
    }

    /// <inheritdoc />
    public async Task<CostEstimate> EstimateAsync(
        RoutingCandidate candidate,
        int inputTokens,
        int outputTokens,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var inputRate = candidate.InputCostPerMillionTokens
                        ?? costRateCatalog.GetInputCostPerMillion(candidate.ProviderKind);
        var outputRate = candidate.OutputCostPerMillionTokens
                         ?? costRateCatalog.GetOutputCostPerMillion(candidate.ProviderKind);

        var inputCost = inputRate * inputTokens / 1_000_000m;
        var outputCost = outputRate * outputTokens / 1_000_000m;
        var gpuRuntimeMs = EstimateGpuRuntimeMs(inputTokens, outputTokens, candidate.SpeedScore);

        var monthStart = new DateTime(dateTimeService.UtcNow.Year, dateTimeService.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthlySpend = await dbContext.CostHistories
            .AsNoTracking()
            .Where(c =>
                c.OrganizationId == organizationId &&
                c.RecordedAt >= monthStart &&
                !c.IsPredicted)
            .SumAsync(c => (decimal?)c.CostUsd, cancellationToken) ?? 0m;

        return new CostEstimate
        {
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            GpuRuntimeMs = gpuRuntimeMs,
            InputCostUsd = Round(inputCost),
            OutputCostUsd = Round(outputCost),
            TotalCostUsd = Round(inputCost + outputCost),
            MonthlySpendUsd = Round(monthlySpend + inputCost + outputCost),
        };
    }

    private static int EstimateGpuRuntimeMs(int inputTokens, int outputTokens, double speedScore)
    {
        var tokensPerSecond = Math.Clamp(speedScore, 10, 100) * 2;
        var totalTokens = inputTokens + outputTokens;
        return (int)Math.Ceiling(totalTokens / tokensPerSecond * 1000.0);
    }

    private static decimal Round(decimal value) => Math.Round(value, 8, MidpointRounding.AwayFromZero);
}
