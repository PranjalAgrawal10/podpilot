namespace PodPilot.Contracts.Observability;

/// <summary>
/// Analytics summary response.
/// </summary>
public sealed class AnalyticsResponse
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>Gets or sets total request count.</summary>
    public int TotalRequests { get; init; }

    /// <summary>Gets or sets total token count.</summary>
    public long TotalTokens { get; init; }

    /// <summary>Gets or sets total inference count.</summary>
    public int TotalInferences { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; init; }

    /// <summary>Gets or sets total uptime in seconds.</summary>
    public long TotalUptimeSeconds { get; init; }

    /// <summary>Gets or sets model usage breakdowns.</summary>
    public IReadOnlyList<ModelUsageBreakdownResponse> ModelBreakdowns { get; init; } = [];

    /// <summary>Gets or sets provider usage breakdowns.</summary>
    public IReadOnlyList<ProviderUsageBreakdownResponse> ProviderBreakdowns { get; init; } = [];

    /// <summary>Gets or sets pod usage breakdowns.</summary>
    public IReadOnlyList<PodUsageBreakdownResponse> PodBreakdowns { get; init; } = [];
}

/// <summary>
/// Model usage breakdown response.
/// </summary>
public sealed class ModelUsageBreakdownResponse
{
    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets token count.</summary>
    public long TokenCount { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }
}

/// <summary>
/// Provider usage breakdown response.
/// </summary>
public sealed class ProviderUsageBreakdownResponse
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets inference count.</summary>
    public int InferenceCount { get; init; }
}

/// <summary>
/// Pod usage breakdown response.
/// </summary>
public sealed class PodUsageBreakdownResponse
{
    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; init; } = string.Empty;

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets uptime in seconds.</summary>
    public long UptimeSeconds { get; init; }
}
