namespace PodPilot.Contracts.Routing;

/// <summary>
/// Intelligent routing dashboard summary.
/// </summary>
public sealed class RoutingDashboardResponse
{
    /// <summary>Gets or sets the current/most recent model.</summary>
    public string? CurrentModel { get; init; }

    /// <summary>Gets or sets the current/most recent provider display name.</summary>
    public string? CurrentProvider { get; init; }

    /// <summary>Gets or sets the current provider identifier.</summary>
    public Guid? CurrentProviderId { get; init; }

    /// <summary>Gets or sets the active strategy.</summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>Gets or sets the latest estimated cost in USD.</summary>
    public decimal EstimatedCostUsd { get; init; }

    /// <summary>Gets or sets the latest estimated latency in milliseconds.</summary>
    public int EstimatedLatencyMs { get; init; }

    /// <summary>Gets or sets total fallback events in the recent window.</summary>
    public int FallbackCount { get; init; }

    /// <summary>Gets or sets most-used models.</summary>
    public IReadOnlyList<RoutingModelUsageItem> MostUsedModels { get; init; } = [];

    /// <summary>Gets or sets provider ranking.</summary>
    public IReadOnlyList<RoutingProviderRankItem> ProviderRanking { get; init; } = [];
}

/// <summary>
/// Model usage count item.
/// </summary>
public sealed class RoutingModelUsageItem
{
    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets the usage count.</summary>
    public int Count { get; init; }
}

/// <summary>
/// Provider ranking item.
/// </summary>
public sealed class RoutingProviderRankItem
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets the average score.</summary>
    public double Score { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public int? LatencyMs { get; init; }

    /// <summary>Gets or sets availability score.</summary>
    public double AvailabilityScore { get; init; }
}
