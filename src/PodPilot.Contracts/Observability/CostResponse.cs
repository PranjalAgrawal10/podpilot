namespace PodPilot.Contracts.Observability;

/// <summary>
/// Cost summary response.
/// </summary>
public sealed class CostResponse
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>Gets or sets when the summary was calculated.</summary>
    public DateTime CalculatedAt { get; init; }

    /// <summary>Gets or sets current hourly cost.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets daily cost.</summary>
    public decimal DailyCost { get; init; }

    /// <summary>Gets or sets weekly cost.</summary>
    public decimal WeeklyCost { get; init; }

    /// <summary>Gets or sets monthly cost.</summary>
    public decimal MonthlyCost { get; init; }

    /// <summary>Gets or sets projected monthly cost.</summary>
    public decimal ProjectedMonthlyCost { get; init; }

    /// <summary>Gets or sets savings from auto shutdown.</summary>
    public decimal AutoShutdownSavings { get; init; }

    /// <summary>Gets or sets per-pod cost breakdowns.</summary>
    public IReadOnlyList<PodCostBreakdownResponse> PodBreakdowns { get; init; } = [];

    /// <summary>Gets or sets per-provider cost breakdowns.</summary>
    public IReadOnlyList<ProviderCostBreakdownResponse> ProviderBreakdowns { get; init; } = [];

    /// <summary>Gets or sets per-model cost breakdowns.</summary>
    public IReadOnlyList<ModelCostBreakdownResponse> ModelBreakdowns { get; init; } = [];
}

/// <summary>
/// Per-pod cost breakdown response.
/// </summary>
public sealed class PodCostBreakdownResponse
{
    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; init; } = string.Empty;

    /// <summary>Gets or sets hourly cost.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets period cost.</summary>
    public decimal PeriodCost { get; init; }
}

/// <summary>
/// Per-provider cost breakdown response.
/// </summary>
public sealed class ProviderCostBreakdownResponse
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets hourly cost.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets period cost.</summary>
    public decimal PeriodCost { get; init; }
}

/// <summary>
/// Per-model cost breakdown response.
/// </summary>
public sealed class ModelCostBreakdownResponse
{
    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets hourly cost allocated to the model.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets period cost allocated to the model.</summary>
    public decimal PeriodCost { get; init; }

    /// <summary>Gets or sets request count used for allocation.</summary>
    public int RequestCount { get; init; }
}
