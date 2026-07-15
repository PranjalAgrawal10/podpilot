using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Routing;

/// <summary>
/// Analyzed characteristics of an incoming AI request.
/// </summary>
public sealed class RoutingRequestAnalysis
{
    /// <summary>Gets or sets the classified task type.</summary>
    public AiTaskType TaskType { get; init; } = AiTaskType.General;

    /// <summary>Gets or sets the estimated complexity.</summary>
    public TaskComplexity Complexity { get; init; } = TaskComplexity.Medium;

    /// <summary>Gets or sets estimated input tokens.</summary>
    public int EstimatedInputTokens { get; init; }

    /// <summary>Gets or sets estimated output tokens.</summary>
    public int EstimatedOutputTokens { get; init; }

    /// <summary>Gets or sets an explicit model hint from the request.</summary>
    public string? RequestedModel { get; init; }

    /// <summary>Gets or sets whether vision capability is required.</summary>
    public bool RequiresVision { get; init; }

    /// <summary>Gets or sets whether embeddings capability is required.</summary>
    public bool RequiresEmbeddings { get; init; }

    /// <summary>Gets or sets whether tool calling is required.</summary>
    public bool RequiresTools { get; init; }

    /// <summary>Gets or sets whether reasoning capability is preferred.</summary>
    public bool RequiresReasoning { get; init; }

    /// <summary>Gets or sets a short prompt preview for logging.</summary>
    public string? PromptPreview { get; init; }
}

/// <summary>
/// Weighted scoring configuration for model ranking.
/// </summary>
public sealed class RoutingScoreWeights
{
    /// <summary>Gets or sets the cost weight.</summary>
    public double Cost { get; init; } = 0.25;

    /// <summary>Gets or sets the latency weight.</summary>
    public double Latency { get; init; } = 0.25;

    /// <summary>Gets or sets the reliability weight.</summary>
    public double Reliability { get; init; } = 0.20;

    /// <summary>Gets or sets the context weight.</summary>
    public double Context { get; init; } = 0.10;

    /// <summary>Gets or sets the features weight.</summary>
    public double Features { get; init; } = 0.10;

    /// <summary>Gets or sets the availability weight.</summary>
    public double Availability { get; init; } = 0.10;

    /// <summary>Gets balanced default weights.</summary>
    public static RoutingScoreWeights Balanced { get; } = new();

    /// <summary>Gets lowest-cost weights.</summary>
    public static RoutingScoreWeights LowestCost { get; } = new()
    {
        Cost = 0.55,
        Latency = 0.10,
        Reliability = 0.15,
        Context = 0.05,
        Features = 0.05,
        Availability = 0.10,
    };

    /// <summary>Gets lowest-latency weights.</summary>
    public static RoutingScoreWeights LowestLatency { get; } = new()
    {
        Cost = 0.10,
        Latency = 0.55,
        Reliability = 0.10,
        Context = 0.05,
        Features = 0.05,
        Availability = 0.15,
    };

    /// <summary>Gets highest-accuracy weights.</summary>
    public static RoutingScoreWeights HighestAccuracy { get; } = new()
    {
        Cost = 0.05,
        Latency = 0.10,
        Reliability = 0.20,
        Context = 0.20,
        Features = 0.35,
        Availability = 0.10,
    };
}

/// <summary>
/// A scored provider/model candidate for routing.
/// </summary>
public sealed class RoutingCandidate
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider display name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets the provider kind.</summary>
    public AiProviderKind ProviderKind { get; init; }

    /// <summary>Gets or sets the provider model catalog identifier.</summary>
    public Guid ModelId { get; init; }

    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets the context length.</summary>
    public int? ContextLength { get; init; }

    /// <summary>Gets or sets whether streaming is supported.</summary>
    public bool SupportsStreaming { get; init; }

    /// <summary>Gets or sets whether vision is supported.</summary>
    public bool SupportsVision { get; init; }

    /// <summary>Gets or sets whether tools are supported.</summary>
    public bool SupportsTools { get; init; }

    /// <summary>Gets or sets whether embeddings are supported.</summary>
    public bool SupportsEmbeddings { get; init; }

    /// <summary>Gets or sets whether reasoning is supported.</summary>
    public bool SupportsReasoning { get; init; }

    /// <summary>Gets or sets input cost per 1M tokens.</summary>
    public decimal? InputCostPerMillionTokens { get; init; }

    /// <summary>Gets or sets output cost per 1M tokens.</summary>
    public decimal? OutputCostPerMillionTokens { get; init; }

    /// <summary>Gets or sets the relative speed score.</summary>
    public double SpeedScore { get; init; }

    /// <summary>Gets or sets the relative quality score.</summary>
    public double QualityScore { get; init; }

    /// <summary>Gets or sets the relative reliability score.</summary>
    public double ReliabilityScore { get; init; } = 50;

    /// <summary>Gets or sets predicted cost in USD.</summary>
    public decimal PredictedCostUsd { get; set; }

    /// <summary>Gets or sets predicted latency in milliseconds.</summary>
    public int PredictedLatencyMs { get; set; }

    /// <summary>Gets or sets the cost component score.</summary>
    public double CostScore { get; set; }

    /// <summary>Gets or sets the latency component score.</summary>
    public double LatencyScore { get; set; }

    /// <summary>Gets or sets the reliability component score.</summary>
    public double ReliabilityComponentScore { get; set; }

    /// <summary>Gets or sets the context component score.</summary>
    public double ContextScore { get; set; }

    /// <summary>Gets or sets the features component score.</summary>
    public double FeaturesScore { get; set; }

    /// <summary>Gets or sets the availability component score.</summary>
    public double AvailabilityScore { get; set; }

    /// <summary>Gets or sets the overall weighted score.</summary>
    public double OverallScore { get; set; }
}

/// <summary>
/// Result of intelligent routing.
/// </summary>
public sealed class RoutingDecision
{
    /// <summary>Gets or sets the selected candidate.</summary>
    public RoutingCandidate? Selected { get; init; }

    /// <summary>Gets or sets ordered fallback candidates.</summary>
    public IReadOnlyList<RoutingCandidate> Fallbacks { get; init; } = [];

    /// <summary>Gets or sets all scored candidates.</summary>
    public IReadOnlyList<RoutingCandidate> ScoredCandidates { get; init; } = [];

    /// <summary>Gets or sets the strategy used.</summary>
    public RoutingStrategy Strategy { get; init; }

    /// <summary>Gets or sets the task type.</summary>
    public AiTaskType TaskType { get; init; }

    /// <summary>Gets or sets the complexity.</summary>
    public TaskComplexity Complexity { get; init; }

    /// <summary>Gets or sets estimated input tokens.</summary>
    public int EstimatedInputTokens { get; init; }

    /// <summary>Gets or sets estimated output tokens.</summary>
    public int EstimatedOutputTokens { get; init; }

    /// <summary>Gets or sets estimated cost in USD.</summary>
    public decimal EstimatedCostUsd { get; init; }

    /// <summary>Gets or sets estimated latency in milliseconds.</summary>
    public int EstimatedLatencyMs { get; init; }

    /// <summary>Gets or sets the policy identifier.</summary>
    public Guid? PolicyId { get; init; }

    /// <summary>Gets or sets the decision reason.</summary>
    public string DecisionReason { get; init; } = string.Empty;

    /// <summary>Gets or sets the fallback count.</summary>
    public int FallbackCount { get; init; }

    /// <summary>Gets or sets a value indicating whether this was a simulation.</summary>
    public bool IsSimulation { get; init; }
}

/// <summary>
/// Input to the routing engine.
/// </summary>
public sealed class RoutingEngineRequest
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the request path.</summary>
    public string? Path { get; init; }

    /// <summary>Gets or sets the request body JSON.</summary>
    public string? BodyJson { get; init; }

    /// <summary>Gets or sets an optional prompt for simulation.</summary>
    public string? Prompt { get; init; }

    /// <summary>Gets or sets an optional strategy override.</summary>
    public RoutingStrategy? StrategyOverride { get; init; }

    /// <summary>Gets or sets an optional model hint.</summary>
    public string? ModelHint { get; init; }

    /// <summary>Gets or sets a value indicating whether this is a simulation.</summary>
    public bool IsSimulation { get; init; }

    /// <summary>Gets or sets an optional gateway request identifier.</summary>
    public Guid? GatewayRequestId { get; init; }
}

/// <summary>
/// Cost estimate for a candidate.
/// </summary>
public sealed class CostEstimate
{
    /// <summary>Gets or sets input tokens.</summary>
    public int InputTokens { get; init; }

    /// <summary>Gets or sets output tokens.</summary>
    public int OutputTokens { get; init; }

    /// <summary>Gets or sets estimated GPU runtime in milliseconds.</summary>
    public int? GpuRuntimeMs { get; init; }

    /// <summary>Gets or sets input cost in USD.</summary>
    public decimal InputCostUsd { get; init; }

    /// <summary>Gets or sets output cost in USD.</summary>
    public decimal OutputCostUsd { get; init; }

    /// <summary>Gets or sets total cost in USD.</summary>
    public decimal TotalCostUsd { get; init; }

    /// <summary>Gets or sets projected monthly spend in USD based on recent history.</summary>
    public decimal MonthlySpendUsd { get; init; }
}

/// <summary>
/// Latency prediction for a provider/model pair.
/// </summary>
public sealed class LatencyPrediction
{
    /// <summary>Gets or sets average response time in milliseconds.</summary>
    public int AverageResponseMs { get; init; }

    /// <summary>Gets or sets current queue depth.</summary>
    public int QueueDepth { get; init; }

    /// <summary>Gets or sets provider health latency sample.</summary>
    public int? ProviderHealthLatencyMs { get; init; }

    /// <summary>Gets or sets estimated pod load percentage.</summary>
    public double PodLoadPercent { get; init; }

    /// <summary>Gets or sets warm pod count.</summary>
    public int WarmPods { get; init; }

    /// <summary>Gets or sets estimated cold-start time in milliseconds.</summary>
    public int ColdStartMs { get; init; }

    /// <summary>Gets or sets predicted end-to-end latency in milliseconds.</summary>
    public int PredictedLatencyMs { get; init; }
}
