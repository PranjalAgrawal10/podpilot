using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Cached weighted score for a provider model candidate.
/// </summary>
public class ModelScore : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; set; }

    /// <summary>Gets or sets the provider model catalog identifier.</summary>
    public Guid AiProviderModelId { get; set; }

    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>Gets or sets the strategy used when scoring.</summary>
    public RoutingStrategy Strategy { get; set; }

    /// <summary>Gets or sets the overall weighted score (0–100).</summary>
    public double OverallScore { get; set; }

    /// <summary>Gets or sets the cost component score (0–100, higher is cheaper).</summary>
    public double CostScore { get; set; }

    /// <summary>Gets or sets the latency component score (0–100, higher is faster).</summary>
    public double LatencyScore { get; set; }

    /// <summary>Gets or sets the reliability component score (0–100).</summary>
    public double ReliabilityScore { get; set; }

    /// <summary>Gets or sets the context-window component score (0–100).</summary>
    public double ContextScore { get; set; }

    /// <summary>Gets or sets the features component score (0–100).</summary>
    public double FeaturesScore { get; set; }

    /// <summary>Gets or sets the availability component score (0–100).</summary>
    public double AvailabilityScore { get; set; }

    /// <summary>Gets or sets when the score was last computed.</summary>
    public DateTime ScoredAt { get; set; }

    /// <summary>Gets the AI provider.</summary>
    public AiInferenceProvider AiProvider { get; set; } = null!;

    /// <summary>Gets the provider model.</summary>
    public AiProviderModel AiProviderModel { get; set; } = null!;
}
