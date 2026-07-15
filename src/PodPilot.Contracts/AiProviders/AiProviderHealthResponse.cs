namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// AI provider health response.
/// </summary>
public sealed class AiProviderHealthResponse
{
    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the health status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets or sets latency in milliseconds.</summary>
    public int? LatencyMs { get; init; }

    /// <summary>Gets or sets the recent error rate.</summary>
    public double ErrorRate { get; init; }

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime LastCheckedAt { get; init; }

    /// <summary>Gets or sets consecutive failure count.</summary>
    public int ConsecutiveFailures { get; init; }
}
