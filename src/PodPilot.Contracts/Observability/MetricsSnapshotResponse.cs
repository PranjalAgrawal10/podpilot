namespace PodPilot.Contracts.Observability;

/// <summary>
/// Metrics snapshot response.
/// </summary>
public sealed class MetricsSnapshotResponse
{
    /// <summary>Gets or sets the snapshot identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets when metrics were recorded.</summary>
    public DateTime RecordedAt { get; init; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets GPU memory used in bytes.</summary>
    public long? GpuMemoryUsedBytes { get; init; }

    /// <summary>Gets or sets GPU memory total in bytes.</summary>
    public long? GpuMemoryTotalBytes { get; init; }

    /// <summary>Gets or sets CPU utilization percentage.</summary>
    public double CpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets RAM used in bytes.</summary>
    public long? MemoryUsedBytes { get; init; }

    /// <summary>Gets or sets RAM total in bytes.</summary>
    public long? MemoryTotalBytes { get; init; }

    /// <summary>Gets or sets disk used in bytes.</summary>
    public long? DiskUsedBytes { get; init; }

    /// <summary>Gets or sets disk total in bytes.</summary>
    public long? DiskTotalBytes { get; init; }

    /// <summary>Gets or sets network inbound bytes.</summary>
    public long NetworkInBytes { get; init; }

    /// <summary>Gets or sets network outbound bytes.</summary>
    public long NetworkOutBytes { get; init; }

    /// <summary>Gets or sets GPU temperature in Celsius.</summary>
    public double? TemperatureCelsius { get; init; }

    /// <summary>Gets or sets power usage in watts.</summary>
    public double? PowerWatts { get; init; }

    /// <summary>Gets or sets active stream count.</summary>
    public int ActiveStreams { get; init; }

    /// <summary>Gets or sets queue size.</summary>
    public int QueueSize { get; init; }

    /// <summary>Gets or sets inference count.</summary>
    public int InferenceCount { get; init; }

    /// <summary>Gets or sets tokens generated.</summary>
    public long TokensGenerated { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; init; }
}
