namespace PodPilot.Domain.Entities;

/// <summary>
/// Historical latency sample for routing predictions.
/// </summary>
public class LatencyHistory : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; set; }

    /// <summary>Gets or sets the model name when known.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets observed latency in milliseconds.</summary>
    public int LatencyMs { get; set; }

    /// <summary>Gets or sets estimated queue depth at sample time.</summary>
    public int QueueDepth { get; set; }

    /// <summary>Gets or sets estimated pod load percentage (0–100).</summary>
    public double PodLoadPercent { get; set; }

    /// <summary>Gets or sets a value indicating whether a cold start was observed.</summary>
    public bool WasColdStart { get; set; }

    /// <summary>Gets or sets cold-start overhead in milliseconds when applicable.</summary>
    public int? ColdStartMs { get; set; }

    /// <summary>Gets or sets when the sample was recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets the AI provider.</summary>
    public AiInferenceProvider AiProvider { get; set; } = null!;
}
