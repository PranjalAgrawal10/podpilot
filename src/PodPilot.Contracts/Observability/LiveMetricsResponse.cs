namespace PodPilot.Contracts.Observability;

/// <summary>
/// Live metrics response for dashboards.
/// </summary>
public sealed class LiveMetricsResponse
{
    /// <summary>Gets or sets when metrics were captured.</summary>
    public DateTime CapturedAt { get; init; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets CPU utilization percentage.</summary>
    public double CpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets active stream count.</summary>
    public int ActiveStreams { get; init; }

    /// <summary>Gets or sets queue size.</summary>
    public int QueueSize { get; init; }

    /// <summary>Gets or sets requests per second.</summary>
    public double RequestsPerSecond { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; init; }

    /// <summary>Gets or sets running pod count.</summary>
    public int RunningPods { get; init; }

    /// <summary>Gets or sets healthy pod count.</summary>
    public int HealthyPods { get; init; }

    /// <summary>Gets or sets failed pod count.</summary>
    public int FailedPods { get; init; }

    /// <summary>Gets or sets stopped pod count.</summary>
    public int StoppedPods { get; init; }

    /// <summary>Gets or sets installed model count.</summary>
    public int ModelsInstalled { get; init; }

    /// <summary>Gets or sets GPU memory used in bytes.</summary>
    public long? GpuMemoryUsedBytes { get; init; }

    /// <summary>Gets or sets GPU memory total in bytes.</summary>
    public long? GpuMemoryTotalBytes { get; init; }

    /// <summary>Gets or sets inference count in the last hour.</summary>
    public int InferenceCountLastHour { get; init; }

    /// <summary>Gets or sets tokens generated in the last hour.</summary>
    public long TokensGeneratedLastHour { get; init; }
}
