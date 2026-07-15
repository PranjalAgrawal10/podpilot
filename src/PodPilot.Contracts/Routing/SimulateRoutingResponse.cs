namespace PodPilot.Contracts.Routing;

/// <summary>
/// Simulation result for intelligent routing.
/// </summary>
public sealed class SimulateRoutingResponse
{
    /// <summary>Gets or sets the classified task type.</summary>
    public string TaskType { get; init; } = string.Empty;

    /// <summary>Gets or sets the estimated complexity.</summary>
    public string Complexity { get; init; } = string.Empty;

    /// <summary>Gets or sets the strategy used.</summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>Gets or sets the predicted provider display name.</summary>
    public string? PredictedProvider { get; init; }

    /// <summary>Gets or sets the predicted provider identifier.</summary>
    public Guid? PredictedProviderId { get; init; }

    /// <summary>Gets or sets the predicted model name.</summary>
    public string? PredictedModel { get; init; }

    /// <summary>Gets or sets estimated cost in USD.</summary>
    public decimal EstimatedCostUsd { get; init; }

    /// <summary>Gets or sets estimated latency in milliseconds.</summary>
    public int EstimatedLatencyMs { get; init; }

    /// <summary>Gets or sets the overall score of the selected candidate.</summary>
    public double? OverallScore { get; init; }

    /// <summary>Gets or sets estimated input tokens.</summary>
    public int EstimatedInputTokens { get; init; }

    /// <summary>Gets or sets estimated output tokens.</summary>
    public int EstimatedOutputTokens { get; init; }

    /// <summary>Gets or sets the decision reason.</summary>
    public string DecisionReason { get; init; } = string.Empty;

    /// <summary>Gets or sets ranked alternative candidates.</summary>
    public IReadOnlyList<RankedModelResponse> RankedAlternatives { get; init; } = [];
}
