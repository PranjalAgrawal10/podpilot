using System.Text.Json;
using PodPilot.Application.Models.Routing;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Routing;

/// <summary>
/// Maps routing entities and decisions to contract responses.
/// </summary>
internal static class RoutingMapper
{
    /// <summary>
    /// Maps a routing policy to settings response.
    /// </summary>
    public static RoutingPolicySettingsResponse ToPolicySettingsResponse(AiRoutingPolicy policy) =>
        new()
        {
            Id = policy.Id,
            Name = policy.Name,
            Strategy = policy.Strategy.ToString(),
            CostWeight = policy.CostWeight,
            LatencyWeight = policy.LatencyWeight,
            ReliabilityWeight = policy.ReliabilityWeight,
            ContextWeight = policy.ContextWeight,
            FeaturesWeight = policy.FeaturesWeight,
            AvailabilityWeight = policy.AvailabilityWeight,
            MaxRetries = policy.MaxRetries,
            FailoverStrategy = policy.FailoverStrategy.ToString(),
            IsDefault = policy.IsDefault,
            PrimaryProviderId = policy.PrimaryProviderId,
            FallbackProviderIds = ParseGuidList(policy.FallbackProviderIdsJson),
            PreferredTaskTypes = ParseStringList(policy.PreferredTaskTypesJson),
            CustomRulesJson = policy.CustomRulesJson,
        };

    /// <summary>
    /// Maps a model score entity to a ranked model response.
    /// </summary>
    public static RankedModelResponse ToRankedModelResponse(ModelScore score) =>
        new()
        {
            ProviderId = score.AiProviderId,
            ProviderName = score.AiProvider?.DisplayName ?? string.Empty,
            ModelId = score.AiProviderModelId,
            ModelName = score.ModelName,
            Strategy = score.Strategy.ToString(),
            OverallScore = score.OverallScore,
            CostScore = score.CostScore,
            LatencyScore = score.LatencyScore,
            ReliabilityScore = score.ReliabilityScore,
            ContextScore = score.ContextScore,
            FeaturesScore = score.FeaturesScore,
            AvailabilityScore = score.AvailabilityScore,
            ScoredAt = score.ScoredAt,
        };

    /// <summary>
    /// Maps a routing candidate to a ranked model response.
    /// </summary>
    public static RankedModelResponse ToRankedModelResponse(RoutingCandidate candidate, string strategy) =>
        new()
        {
            ProviderId = candidate.ProviderId,
            ProviderName = candidate.ProviderName,
            ModelId = candidate.ModelId,
            ModelName = candidate.ModelName,
            Strategy = strategy,
            OverallScore = candidate.OverallScore,
            CostScore = candidate.CostScore,
            LatencyScore = candidate.LatencyScore,
            ReliabilityScore = candidate.ReliabilityComponentScore,
            ContextScore = candidate.ContextScore,
            FeaturesScore = candidate.FeaturesScore,
            AvailabilityScore = candidate.AvailabilityScore,
            ScoredAt = DateTime.UtcNow,
        };

    /// <summary>
    /// Maps a routing event to a history item response.
    /// </summary>
    public static RoutingHistoryItemResponse ToHistoryItemResponse(RoutingEvent routingEvent) =>
        new()
        {
            Id = routingEvent.Id,
            TaskType = routingEvent.TaskType.ToString(),
            Complexity = routingEvent.Complexity.ToString(),
            Strategy = routingEvent.Strategy.ToString(),
            SelectedProviderId = routingEvent.SelectedProviderId,
            SelectedProviderName = routingEvent.SelectedProvider?.DisplayName,
            SelectedModelName = routingEvent.SelectedModelName,
            OverallScore = routingEvent.OverallScore,
            EstimatedCostUsd = routingEvent.EstimatedCostUsd,
            EstimatedLatencyMs = routingEvent.EstimatedLatencyMs,
            FallbackCount = routingEvent.FallbackCount,
            IsSimulation = routingEvent.IsSimulation,
            DecisionReason = routingEvent.DecisionReason,
            DecidedAt = routingEvent.DecidedAt,
        };

    /// <summary>
    /// Maps a routing decision to a simulation response.
    /// </summary>
    public static SimulateRoutingResponse ToSimulateResponse(RoutingDecision decision) =>
        new()
        {
            TaskType = decision.TaskType.ToString(),
            Complexity = decision.Complexity.ToString(),
            Strategy = decision.Strategy.ToString(),
            PredictedProvider = decision.Selected?.ProviderName,
            PredictedProviderId = decision.Selected?.ProviderId,
            PredictedModel = decision.Selected?.ModelName,
            EstimatedCostUsd = decision.EstimatedCostUsd,
            EstimatedLatencyMs = decision.EstimatedLatencyMs,
            OverallScore = decision.Selected?.OverallScore,
            EstimatedInputTokens = decision.EstimatedInputTokens,
            EstimatedOutputTokens = decision.EstimatedOutputTokens,
            DecisionReason = decision.DecisionReason,
            RankedAlternatives = decision.ScoredCandidates
                .Take(10)
                .Select(c => ToRankedModelResponse(c, decision.Strategy.ToString()))
                .ToList(),
        };

    /// <summary>
    /// Serializes preferred task types to JSON.
    /// </summary>
    public static string ToPreferredTaskTypesJson(IReadOnlyList<string>? taskTypes) =>
        JsonSerializer.Serialize(taskTypes?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList() ?? []);

    /// <summary>
    /// Serializes fallback provider IDs to JSON.
    /// </summary>
    public static string ToFallbackJson(IReadOnlyList<Guid>? ids) =>
        JsonSerializer.Serialize(ids?.ToList() ?? []);

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

    private static IReadOnlyList<string> ParseStringList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
