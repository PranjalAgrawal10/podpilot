namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// AI provider dashboard summary response.
/// </summary>
public sealed class AiProviderDashboardResponse
{
    /// <summary>Gets or sets connected (healthy) provider count.</summary>
    public int ConnectedProviders { get; init; }

    /// <summary>Gets or sets total enabled providers.</summary>
    public int TotalProviders { get; init; }

    /// <summary>Gets or sets available model count.</summary>
    public int AvailableModels { get; init; }

    /// <summary>Gets or sets unhealthy provider count.</summary>
    public int UnhealthyProviders { get; init; }

    /// <summary>Gets or sets active streaming sessions estimate.</summary>
    public int StreamingSessions { get; init; }

    /// <summary>Gets or sets average provider latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets average provider error rate.</summary>
    public double AverageErrorRate { get; init; }
}
