using PodPilot.Application.Models.AiProviders;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Scores routing candidates using weighted metrics.
/// </summary>
public interface IModelScorer
{
    /// <summary>Applies component and overall scores to candidates.</summary>
    IReadOnlyList<RoutingCandidate> Score(
        IReadOnlyList<RoutingCandidate> candidates,
        RoutingRequestAnalysis analysis,
        RoutingScoreWeights weights);
}

/// <summary>
/// Selects the best model from scored candidates.
/// </summary>
public interface IModelRouter
{
    /// <summary>Selects the best model candidate.</summary>
    Task<RoutingCandidate?> SelectModelAsync(
        IReadOnlyList<RoutingCandidate> candidates,
        RoutingRequestAnalysis analysis,
        RoutingScoreWeights weights,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Loads and filters provider/model candidates for routing.
/// </summary>
public interface IProviderSelector
{
    /// <summary>Selects candidate provider/model pairs for the analysis.</summary>
    Task<IReadOnlyList<RoutingCandidate>> SelectProvidersAsync(
        Guid organizationId,
        RoutingRequestAnalysis analysis,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Looks up the active organization routing policy.
/// </summary>
public interface IRoutingPolicy
{
    /// <summary>Gets the active policy for an organization (model match → default).</summary>
    Task<AiRoutingPolicy?> GetActivePolicyAsync(
        Guid organizationId,
        string? modelHint,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves scoring weights for a routing strategy (open for new strategies).
/// </summary>
public interface IRoutingWeightResolver
{
    /// <summary>Resolves scoring weights for a policy and strategy.</summary>
    RoutingScoreWeights Resolve(AiRoutingPolicy? policy, RoutingStrategy strategy);
}

/// <summary>
/// Provides default provider pricing when catalog costs are absent.
/// </summary>
public interface IProviderCostRateCatalog
{
    /// <summary>Gets default input USD per 1M tokens.</summary>
    decimal GetInputCostPerMillion(AiProviderKind providerKind);

    /// <summary>Gets default output USD per 1M tokens.</summary>
    decimal GetOutputCostPerMillion(AiProviderKind providerKind);
}

/// <summary>
/// Enriches candidates with predicted cost, latency, and availability.
/// </summary>
public interface IRoutingCandidateEnricher
{
    /// <summary>Enriches a candidate in place.</summary>
    Task EnrichAsync(
        Guid organizationId,
        RoutingCandidate candidate,
        RoutingRequestAnalysis analysis,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Plans a routing decision for a supported strategy.
/// </summary>
public interface IRoutePlanner
{
    /// <summary>Gets a value indicating whether this planner can handle the strategy/policy.</summary>
    bool CanHandle(RoutingStrategy strategy, AiRoutingPolicy? policy);

    /// <summary>Plans the routing decision.</summary>
    Task<RoutingDecision> PlanAsync(RoutingPlanContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for route planners.
/// </summary>
public sealed class RoutingPlanContext
{
    /// <summary>Gets or sets the engine request.</summary>
    required public RoutingEngineRequest Request { get; init; }

    /// <summary>Gets or sets the request analysis.</summary>
    required public RoutingRequestAnalysis Analysis { get; init; }

    /// <summary>Gets or sets the active policy.</summary>
    public AiRoutingPolicy? Policy { get; init; }

    /// <summary>Gets or sets the effective strategy.</summary>
    required public RoutingStrategy Strategy { get; init; }
}

/// <summary>
/// Persists routing decisions and observed outcomes.
/// </summary>
public interface IRoutingDecisionStore
{
    /// <summary>Persists a routing decision and refreshes model scores.</summary>
    Task PersistDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        Guid? gatewayRequestId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Records observed latency and cost after execution.</summary>
    Task RecordOutcomeAsync(
        Guid organizationId,
        Guid providerId,
        string? modelName,
        int latencyMs,
        int inputTokens,
        int outputTokens,
        decimal? actualCostUsd = null,
        bool wasColdStart = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Estimates request cost for a routing candidate.
/// </summary>
public interface ICostEstimator
{
    /// <summary>Estimates cost for the given candidate and token counts.</summary>
    Task<CostEstimate> EstimateAsync(
        RoutingCandidate candidate,
        int inputTokens,
        int outputTokens,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Predicts latency for a provider/model pair.
/// </summary>
public interface ILatencyPredictor
{
    /// <summary>Predicts latency for the provider and optional model.</summary>
    Task<LatencyPrediction> PredictAsync(
        Guid organizationId,
        Guid providerId,
        string? modelName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Scores provider availability for routing.
/// </summary>
public interface IAvailabilityScorer
{
    /// <summary>Returns an availability score from 0–100.</summary>
    Task<double> ScoreAsync(
        Guid organizationId,
        Guid providerId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Orchestrates intelligent routing: analyze → plan → persist.
/// </summary>
public interface IRoutingEngine
{
    /// <summary>Routes a live or simulated request.</summary>
    Task<RoutingDecision> RouteAsync(
        RoutingEngineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Simulates routing without affecting live traffic.</summary>
    Task<RoutingDecision> SimulateAsync(
        RoutingEngineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a routing decision and refreshes model scores.</summary>
    Task PersistDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        Guid? gatewayRequestId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Records observed latency and cost after execution.</summary>
    Task RecordOutcomeAsync(
        Guid organizationId,
        Guid providerId,
        string? modelName,
        int latencyMs,
        int inputTokens,
        int outputTokens,
        decimal? actualCostUsd = null,
        bool wasColdStart = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Classifies AI requests into task type, complexity, and token estimates.
/// </summary>
public interface ITaskClassifier
{
    /// <summary>Analyzes a request path, body, and optional prompt.</summary>
    RoutingRequestAnalysis Analyze(string? path, string? bodyJson, string? prompt);
}

/// <summary>
/// Legacy static policy/catalog route resolution.
/// </summary>
public interface ILegacyAiInferenceRouter
{
    /// <summary>Attempts to resolve a route using static policies and catalog priority.</summary>
    Task<AiInferenceRoute?> TryResolveAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SignalR notifications for intelligent routing.
/// </summary>
public interface IRoutingNotificationService
{
    /// <summary>Notifies that a routing decision was made.</summary>
    Task NotifyRoutingDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies that the active provider changed.</summary>
    Task NotifyProviderChangedAsync(
        Guid organizationId,
        Guid? previousProviderId,
        Guid providerId,
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies that a fallback occurred.</summary>
    Task NotifyFallbackOccurredAsync(
        Guid organizationId,
        Guid fromProviderId,
        Guid? toProviderId,
        string? modelName,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies that routing policy settings were updated.</summary>
    Task NotifyPolicyUpdatedAsync(
        Guid organizationId,
        Guid policyId,
        CancellationToken cancellationToken = default);
}
