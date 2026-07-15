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

    /// <summary>
    /// Initializes a new instance of the <see cref="CostEstimator"/> class.
    /// </summary>
    public CostEstimator(IApplicationDbContext dbContext, IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<CostEstimate> EstimateAsync(
        RoutingCandidate candidate,
        int inputTokens,
        int outputTokens,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var inputRate = candidate.InputCostPerMillionTokens ?? DefaultInputRate(candidate);
        var outputRate = candidate.OutputCostPerMillionTokens ?? DefaultOutputRate(candidate);

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

    private static decimal DefaultInputRate(RoutingCandidate candidate) =>
        candidate.ProviderKind switch
        {
            Domain.Enums.AiProviderKind.Ollama => 0m,
            Domain.Enums.AiProviderKind.Vllm => 0.1m,
            Domain.Enums.AiProviderKind.LlamaCpp => 0.1m,
            Domain.Enums.AiProviderKind.Groq => 0.2m,
            Domain.Enums.AiProviderKind.OpenRouter => 1.5m,
            Domain.Enums.AiProviderKind.Anthropic => 3m,
            Domain.Enums.AiProviderKind.OpenAi => 2.5m,
            Domain.Enums.AiProviderKind.AzureOpenAi => 2.5m,
            Domain.Enums.AiProviderKind.GoogleGemini => 1.25m,
            _ => 1m,
        };

    private static decimal DefaultOutputRate(RoutingCandidate candidate) =>
        candidate.ProviderKind switch
        {
            Domain.Enums.AiProviderKind.Ollama => 0m,
            Domain.Enums.AiProviderKind.Vllm => 0.2m,
            Domain.Enums.AiProviderKind.LlamaCpp => 0.2m,
            Domain.Enums.AiProviderKind.Groq => 0.5m,
            Domain.Enums.AiProviderKind.OpenRouter => 4m,
            Domain.Enums.AiProviderKind.Anthropic => 15m,
            Domain.Enums.AiProviderKind.OpenAi => 10m,
            Domain.Enums.AiProviderKind.AzureOpenAi => 10m,
            Domain.Enums.AiProviderKind.GoogleGemini => 5m,
            _ => 3m,
        };

    private static int EstimateGpuRuntimeMs(int inputTokens, int outputTokens, double speedScore)
    {
        var tokensPerSecond = Math.Clamp(speedScore, 10, 100) * 2;
        var totalTokens = inputTokens + outputTokens;
        return (int)Math.Ceiling(totalTokens / tokensPerSecond * 1000.0);
    }

    private static decimal Round(decimal value) => Math.Round(value, 8, MidpointRounding.AwayFromZero);
}
