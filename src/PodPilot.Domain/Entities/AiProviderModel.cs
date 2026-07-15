namespace PodPilot.Domain.Entities;

/// <summary>
/// Unified model catalog entry for an AI inference provider.
/// </summary>
public class AiProviderModel : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; set; }

    /// <summary>Gets or sets the provider-native model name.</summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional display name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the context length in tokens.</summary>
    public int? ContextLength { get; set; }

    /// <summary>Gets or sets the parameter count label (e.g. 70B).</summary>
    public string? Parameters { get; set; }

    /// <summary>Gets or sets whether streaming is supported.</summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>Gets or sets whether vision inputs are supported.</summary>
    public bool SupportsVision { get; set; }

    /// <summary>Gets or sets whether tool/function calling is supported.</summary>
    public bool SupportsTools { get; set; }

    /// <summary>Gets or sets whether embeddings are supported.</summary>
    public bool SupportsEmbeddings { get; set; }

    /// <summary>Gets or sets whether advanced reasoning is supported.</summary>
    public bool SupportsReasoning { get; set; }

    /// <summary>Gets or sets a relative speed score (0–100, higher is faster).</summary>
    public double SpeedScore { get; set; } = 50;

    /// <summary>Gets or sets a relative quality/accuracy score (0–100).</summary>
    public double QualityScore { get; set; } = 50;

    /// <summary>Gets or sets a relative reliability score (0–100).</summary>
    public double ReliabilityScore { get; set; } = 50;

    /// <summary>Gets or sets optional input cost per 1M tokens.</summary>
    public decimal? InputCostPerMillionTokens { get; set; }

    /// <summary>Gets or sets optional output cost per 1M tokens.</summary>
    public decimal? OutputCostPerMillionTokens { get; set; }

    /// <summary>Gets or sets a value indicating whether the model is enabled for routing.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets when the catalog entry was last synced.</summary>
    public DateTime SyncedAt { get; set; }

    /// <summary>Gets the AI provider.</summary>
    public AiInferenceProvider AiProvider { get; set; } = null!;
}
