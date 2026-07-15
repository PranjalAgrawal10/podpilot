using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing.Planners;

/// <summary>
/// Plans routes by enriching, scoring, and selecting the best candidate.
/// </summary>
public sealed class ScoredRoutePlanner : IRoutePlanner
{
    private readonly IProviderSelector providerSelector;
    private readonly IRoutingCandidateEnricher candidateEnricher;
    private readonly IRoutingWeightResolver weightResolver;
    private readonly IModelScorer modelScorer;
    private readonly IModelRouter modelRouter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScoredRoutePlanner"/> class.
    /// </summary>
    public ScoredRoutePlanner(
        IProviderSelector providerSelector,
        IRoutingCandidateEnricher candidateEnricher,
        IRoutingWeightResolver weightResolver,
        IModelScorer modelScorer,
        IModelRouter modelRouter)
    {
        this.providerSelector = providerSelector;
        this.candidateEnricher = candidateEnricher;
        this.weightResolver = weightResolver;
        this.modelScorer = modelScorer;
        this.modelRouter = modelRouter;
    }

    /// <inheritdoc />
    public bool CanHandle(RoutingStrategy strategy, Domain.Entities.AiRoutingPolicy? policy) =>
        strategy != RoutingStrategy.ProviderPriority || policy?.PrimaryProviderId is null;

    /// <inheritdoc />
    public async Task<RoutingDecision> PlanAsync(
        RoutingPlanContext context,
        CancellationToken cancellationToken = default)
    {
        var analysis = context.Analysis;
        var strategy = context.Strategy;
        var weights = weightResolver.Resolve(context.Policy, strategy);
        var candidates = (await providerSelector.SelectProvidersAsync(
            context.Request.OrganizationId,
            analysis,
            cancellationToken)).ToList();

        if (candidates.Count == 0)
        {
            return new RoutingDecision
            {
                Strategy = strategy,
                TaskType = analysis.TaskType,
                Complexity = analysis.Complexity,
                EstimatedInputTokens = analysis.EstimatedInputTokens,
                EstimatedOutputTokens = analysis.EstimatedOutputTokens,
                PolicyId = context.Policy?.Id,
                DecisionReason = "No eligible provider models matched the request capabilities.",
                IsSimulation = context.Request.IsSimulation,
            };
        }

        foreach (var candidate in candidates)
        {
            await candidateEnricher.EnrichAsync(
                context.Request.OrganizationId,
                candidate,
                analysis,
                cancellationToken);
        }

        var scored = modelScorer.Score(candidates, analysis, weights);
        var selected = await modelRouter.SelectModelAsync(scored, analysis, weights, cancellationToken);
        var ordered = scored.ToList();
        var fallbacks = ordered.Where(c => selected is null || c.ModelId != selected.ModelId).Take(5).ToList();

        return new RoutingDecision
        {
            Selected = selected,
            Fallbacks = fallbacks,
            ScoredCandidates = ordered,
            Strategy = strategy,
            TaskType = analysis.TaskType,
            Complexity = analysis.Complexity,
            EstimatedInputTokens = analysis.EstimatedInputTokens,
            EstimatedOutputTokens = analysis.EstimatedOutputTokens,
            EstimatedCostUsd = selected?.PredictedCostUsd ?? 0,
            EstimatedLatencyMs = selected?.PredictedLatencyMs ?? 0,
            PolicyId = context.Policy?.Id,
            DecisionReason = selected is null
                ? "No candidate could be selected."
                : $"Selected {selected.ModelName} on {selected.ProviderName} via {strategy} (score {selected.OverallScore:F1}).",
            FallbackCount = 0,
            IsSimulation = context.Request.IsSimulation,
        };
    }
}
