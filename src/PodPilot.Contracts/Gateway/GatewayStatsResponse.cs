namespace PodPilot.Contracts.Gateway;

/// <summary>
/// Gateway dashboard statistics.
/// </summary>
public sealed class GatewayStatsResponse
{
    /// <summary>
    /// Gets or sets active request count.
    /// </summary>
    public int ActiveRequests { get; set; }

    /// <summary>
    /// Gets or sets streaming request count.
    /// </summary>
    public int StreamingRequests { get; set; }

    /// <summary>
    /// Gets or sets requests waiting on pod wake/health.
    /// </summary>
    public int WaitingPods { get; set; }

    /// <summary>
    /// Gets or sets average latency in milliseconds.
    /// </summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets failed request count in the last hour.
    /// </summary>
    public int RecentErrors { get; set; }
}
