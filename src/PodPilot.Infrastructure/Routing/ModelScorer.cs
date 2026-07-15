using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Applies weighted scoring to routing candidates.
/// </summary>
public sealed class ModelScorer : IModelScorer
{
    /// <inheritdoc />
    public IReadOnlyList<RoutingCandidate> Score(
        IReadOnlyList<RoutingCandidate> candidates,
        RoutingRequestAnalysis analysis,
        RoutingScoreWeights weights)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var maxCost = candidates.Max(c => c.PredictedCostUsd);
        var maxLatency = candidates.Max(c => c.PredictedLatencyMs);
        var maxContext = candidates.Max(c => c.ContextLength ?? 0);
        if (maxCost <= 0)
        {
            maxCost = 1;
        }

        if (maxLatency <= 0)
        {
            maxLatency = 1;
        }

        if (maxContext <= 0)
        {
            maxContext = 1;
        }

        foreach (var candidate in candidates)
        {
            candidate.CostScore = (double)(1m - (candidate.PredictedCostUsd / maxCost)) * 100.0;
            candidate.LatencyScore = (1.0 - (candidate.PredictedLatencyMs / (double)maxLatency)) * 100.0;
            candidate.ReliabilityComponentScore = candidate.ReliabilityScore;
            candidate.ContextScore = ((candidate.ContextLength ?? 0) / (double)maxContext) * 100.0;
            candidate.FeaturesScore = ComputeFeatureScore(candidate, analysis);

            candidate.OverallScore =
                (candidate.CostScore * weights.Cost) +
                (candidate.LatencyScore * weights.Latency) +
                (candidate.ReliabilityComponentScore * weights.Reliability) +
                (candidate.ContextScore * weights.Context) +
                (candidate.FeaturesScore * weights.Features) +
                (candidate.AvailabilityScore * weights.Availability);

            if (!string.IsNullOrWhiteSpace(analysis.RequestedModel) &&
                string.Equals(candidate.ModelName, analysis.RequestedModel, StringComparison.OrdinalIgnoreCase))
            {
                candidate.OverallScore += 5;
            }
        }

        return candidates.OrderByDescending(c => c.OverallScore).ToList();
    }

    private static double ComputeFeatureScore(RoutingCandidate candidate, RoutingRequestAnalysis analysis)
    {
        var score = candidate.QualityScore * 0.5;
        score += candidate.SpeedScore * 0.2;

        if (analysis.RequiresVision)
        {
            score += candidate.SupportsVision ? 20 : -40;
        }

        if (analysis.RequiresEmbeddings)
        {
            score += candidate.SupportsEmbeddings ? 25 : -50;
        }

        if (analysis.RequiresTools)
        {
            score += candidate.SupportsTools ? 15 : -20;
        }

        if (analysis.RequiresReasoning || analysis.TaskType == AiTaskType.Reasoning)
        {
            score += candidate.SupportsReasoning ? 20 : 0;
        }

        if (analysis.Complexity == TaskComplexity.High && (candidate.ContextLength ?? 0) >= 32000)
        {
            score += 10;
        }

        return Math.Clamp(score, 0, 100);
    }
}
