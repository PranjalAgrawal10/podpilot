namespace PodPilot.Domain.Entities;

/// <summary>
/// Point-in-time metrics snapshot for an organization.
/// </summary>
public class MetricsSnapshot : Common.BaseEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets when metrics were recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; set; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; set; }

    /// <summary>Gets or sets GPU memory used in bytes.</summary>
    public long? GpuMemoryUsedBytes { get; set; }

    /// <summary>Gets or sets GPU memory total in bytes.</summary>
    public long? GpuMemoryTotalBytes { get; set; }

    /// <summary>Gets or sets CPU utilization percentage.</summary>
    public double CpuUtilizationPercent { get; set; }

    /// <summary>Gets or sets RAM used in bytes.</summary>
    public long? MemoryUsedBytes { get; set; }

    /// <summary>Gets or sets RAM total in bytes.</summary>
    public long? MemoryTotalBytes { get; set; }

    /// <summary>Gets or sets disk used in bytes.</summary>
    public long? DiskUsedBytes { get; set; }

    /// <summary>Gets or sets disk total in bytes.</summary>
    public long? DiskTotalBytes { get; set; }

    /// <summary>Gets or sets network inbound bytes.</summary>
    public long NetworkInBytes { get; set; }

    /// <summary>Gets or sets network outbound bytes.</summary>
    public long NetworkOutBytes { get; set; }

    /// <summary>Gets or sets GPU temperature in Celsius.</summary>
    public double? TemperatureCelsius { get; set; }

    /// <summary>Gets or sets power usage in watts.</summary>
    public double? PowerWatts { get; set; }

    /// <summary>Gets or sets active stream count.</summary>
    public int ActiveStreams { get; set; }

    /// <summary>Gets or sets queue size.</summary>
    public int QueueSize { get; set; }

    /// <summary>Gets or sets inference count in the period.</summary>
    public int InferenceCount { get; set; }

    /// <summary>Gets or sets tokens generated in the period.</summary>
    public long TokensGenerated { get; set; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; set; }
}
