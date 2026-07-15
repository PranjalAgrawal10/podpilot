namespace PodPilot.Contracts.Routing;

/// <summary>
/// A historical routing decision.
/// </summary>
public sealed class RoutingHistoryItemResponse
{
    /// <summary>Gets or sets the event identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the task type.</summary>
    public string TaskType { get; init; } = string.Empty;

    /// <summary>Gets or sets the complexity.</summary>
    public string Complexity { get; init; } = string.Empty;

    /// <summary>Gets or sets the strategy.</summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>Gets or sets the selected provider identifier.</summary>
    public Guid? SelectedProviderId { get; init; }

    /// <summary>Gets or sets the selected provider name.</summary>
    public string? SelectedProviderName { get; init; }

    /// <summary>Gets or sets the selected model name.</summary>
    public string? SelectedModelName { get; init; }

    /// <summary>Gets or sets the overall score.</summary>
    public double? OverallScore { get; init; }

    /// <summary>Gets or sets estimated cost in USD.</summary>
    public decimal EstimatedCostUsd { get; init; }

    /// <summary>Gets or sets estimated latency in milliseconds.</summary>
    public int EstimatedLatencyMs { get; init; }

    /// <summary>Gets or sets the fallback count.</summary>
    public int FallbackCount { get; init; }

    /// <summary>Gets or sets a value indicating whether this was a simulation.</summary>
    public bool IsSimulation { get; init; }

    /// <summary>Gets or sets the decision reason.</summary>
    public string? DecisionReason { get; init; }

    /// <summary>Gets or sets when the decision was made.</summary>
    public DateTime DecidedAt { get; init; }
}
