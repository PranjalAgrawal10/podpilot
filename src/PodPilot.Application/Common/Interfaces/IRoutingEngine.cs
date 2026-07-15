using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

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
/// Resolves active routing policies and scoring weights.
/// </summary>
public interface IRoutingPolicy
{
    /// <summary>Gets the active policy for an organization (model match → default).</summary>
    Task<AiRoutingPolicy?> GetActivePolicyAsync(
        Guid organizationId,
        string? modelHint,
        CancellationToken cancellationToken = default);

    /// <summary>Resolves scoring weights for a policy and strategy.</summary>
    RoutingScoreWeights GetWeights(AiRoutingPolicy? policy, RoutingStrategy strategy);
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
/// Orchestrates intelligent routing: analyze → score → select → persist.
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
