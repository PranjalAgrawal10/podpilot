namespace PodPilot.Contracts.Routing;

/// <summary>
/// Ranked model score for the routing catalog.
/// </summary>
public sealed class RankedModelResponse
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider display name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets the model catalog identifier.</summary>
    public Guid ModelId { get; init; }

    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets the strategy used when scored.</summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>Gets or sets the overall score.</summary>
    public double OverallScore { get; init; }

    /// <summary>Gets or sets the cost score.</summary>
    public double CostScore { get; init; }

    /// <summary>Gets or sets the latency score.</summary>
    public double LatencyScore { get; init; }

    /// <summary>Gets or sets the reliability score.</summary>
    public double ReliabilityScore { get; init; }

    /// <summary>Gets or sets the context score.</summary>
    public double ContextScore { get; init; }

    /// <summary>Gets or sets the features score.</summary>
    public double FeaturesScore { get; init; }

    /// <summary>Gets or sets the availability score.</summary>
    public double AvailabilityScore { get; init; }

    /// <summary>Gets or sets when the score was computed.</summary>
    public DateTime ScoredAt { get; init; }
}
