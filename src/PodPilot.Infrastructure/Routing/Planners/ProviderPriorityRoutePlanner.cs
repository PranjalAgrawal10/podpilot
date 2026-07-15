using System.Text.Json;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing.Planners;

/// <summary>
/// Plans routes using an explicit primary provider and ordered fallbacks.
/// </summary>
public sealed class ProviderPriorityRoutePlanner : IRoutePlanner
{
    private readonly IProviderSelector providerSelector;
    private readonly IRoutingCandidateEnricher candidateEnricher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderPriorityRoutePlanner"/> class.
    /// </summary>
    public ProviderPriorityRoutePlanner(
        IProviderSelector providerSelector,
        IRoutingCandidateEnricher candidateEnricher)
    {
        this.providerSelector = providerSelector;
        this.candidateEnricher = candidateEnricher;
    }

    /// <inheritdoc />
    public bool CanHandle(RoutingStrategy strategy, Domain.Entities.AiRoutingPolicy? policy) =>
        strategy == RoutingStrategy.ProviderPriority && policy?.PrimaryProviderId is not null;

    /// <inheritdoc />
    public async Task<RoutingDecision> PlanAsync(
        RoutingPlanContext context,
        CancellationToken cancellationToken = default)
    {
        var policy = context.Policy!;
        var primaryProviderId = policy.PrimaryProviderId!.Value;
        var analysis = context.Analysis;
        var all = await providerSelector.SelectProvidersAsync(
            context.Request.OrganizationId,
            analysis,
            cancellationToken);

        var ordered = new List<RoutingCandidate>();
        ordered.AddRange(all.Where(c => c.ProviderId == primaryProviderId));

        foreach (var fallbackId in ParseGuidList(policy.FallbackProviderIdsJson))
        {
            ordered.AddRange(all.Where(c =>
                c.ProviderId == fallbackId &&
                ordered.All(o => o.ModelId != c.ModelId)));
        }

        if (ordered.Count == 0)
        {
            ordered.AddRange(all);
        }

        for (var i = 0; i < ordered.Count; i++)
        {
            var candidate = ordered[i];
            await candidateEnricher.EnrichAsync(
                context.Request.OrganizationId,
                candidate,
                analysis,
                cancellationToken);
            candidate.OverallScore = 100 - (i * 5);
        }

        var selected = ordered.FirstOrDefault();
        return new RoutingDecision
        {
            Selected = selected,
            Fallbacks = ordered.Skip(1).Take(5).ToList(),
            ScoredCandidates = ordered,
            Strategy = context.Strategy,
            TaskType = analysis.TaskType,
            Complexity = analysis.Complexity,
            EstimatedInputTokens = analysis.EstimatedInputTokens,
            EstimatedOutputTokens = analysis.EstimatedOutputTokens,
            EstimatedCostUsd = selected?.PredictedCostUsd ?? 0,
            EstimatedLatencyMs = selected?.PredictedLatencyMs ?? 0,
            PolicyId = policy.Id,
            DecisionReason = selected is null
                ? "ProviderPriority policy had no matching models."
                : $"ProviderPriority selected {selected.ModelName} on {selected.ProviderName}.",
            IsSimulation = context.Request.IsSimulation,
        };
    }

    private static IReadOnlyList<Guid> ParseGuidList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
