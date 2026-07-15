namespace PodPilot.Domain.Entities;

/// <summary>
/// Historical cost sample for routing predictions and spend tracking.
/// </summary>
public class CostHistory : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; set; }

    /// <summary>Gets or sets the model name when known.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets estimated or actual input tokens.</summary>
    public int InputTokens { get; set; }

    /// <summary>Gets or sets estimated or actual output tokens.</summary>
    public int OutputTokens { get; set; }

    /// <summary>Gets or sets estimated GPU runtime in milliseconds.</summary>
    public int? GpuRuntimeMs { get; set; }

    /// <summary>Gets or sets the predicted or actual cost in USD.</summary>
    public decimal CostUsd { get; set; }

    /// <summary>Gets or sets a value indicating whether the cost was predicted (vs actual).</summary>
    public bool IsPredicted { get; set; }

    /// <summary>Gets or sets when the sample was recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets the AI provider.</summary>
    public AiInferenceProvider AiProvider { get; set; } = null!;
}
