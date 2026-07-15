using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Current health snapshot for an AI inference provider.
/// </summary>
public class AiProviderHealth : Common.BaseEntity
{
    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; set; }

    /// <summary>Gets or sets the health state.</summary>
    public AiProviderHealthState Status { get; set; } = AiProviderHealthState.Unknown;

    /// <summary>Gets or sets the measured latency in milliseconds.</summary>
    public int? LatencyMs { get; set; }

    /// <summary>Gets or sets the recent error rate (0-1).</summary>
    public double ErrorRate { get; set; }

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime LastCheckedAt { get; set; }

    /// <summary>Gets or sets consecutive failure count.</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>Gets the AI provider.</summary>
    public AiInferenceProvider AiProvider { get; set; } = null!;
}
