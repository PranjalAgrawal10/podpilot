using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Enriches candidates with cost, latency, and availability predictions.
/// </summary>
public sealed class RoutingCandidateEnricher : IRoutingCandidateEnricher
{
    private readonly ICostEstimator costEstimator;
    private readonly ILatencyPredictor latencyPredictor;
    private readonly IAvailabilityScorer availabilityScorer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingCandidateEnricher"/> class.
    /// </summary>
    public RoutingCandidateEnricher(
        ICostEstimator costEstimator,
        ILatencyPredictor latencyPredictor,
        IAvailabilityScorer availabilityScorer)
    {
        this.costEstimator = costEstimator;
        this.latencyPredictor = latencyPredictor;
        this.availabilityScorer = availabilityScorer;
    }

    /// <inheritdoc />
    public async Task EnrichAsync(
        Guid organizationId,
        RoutingCandidate candidate,
        RoutingRequestAnalysis analysis,
        CancellationToken cancellationToken = default)
    {
        var cost = await costEstimator.EstimateAsync(
            candidate,
            analysis.EstimatedInputTokens,
            analysis.EstimatedOutputTokens,
            organizationId,
            cancellationToken);
        var latency = await latencyPredictor.PredictAsync(
            organizationId,
            candidate.ProviderId,
            candidate.ModelName,
            cancellationToken);

        candidate.PredictedCostUsd = cost.TotalCostUsd;
        candidate.PredictedLatencyMs = latency.PredictedLatencyMs;
        candidate.AvailabilityScore = await availabilityScorer.ScoreAsync(
            organizationId,
            candidate.ProviderId,
            cancellationToken);
    }
}
