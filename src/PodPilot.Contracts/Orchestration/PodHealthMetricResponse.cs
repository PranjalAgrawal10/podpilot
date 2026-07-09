namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Pod health metric response.
/// </summary>
public sealed class PodHealthMetricResponse
{
    /// <summary>Gets or sets the metric identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the GPU pod identifier.</summary>
    public Guid GpuPodId { get; set; }

    /// <summary>Gets or sets when metrics were recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether the GPU is healthy.</summary>
    public bool GpuHealthy { get; set; }

    /// <summary>Gets or sets a value indicating whether Ollama is healthy.</summary>
    public bool OllamaHealthy { get; set; }

    /// <summary>Gets or sets a value indicating whether models are healthy.</summary>
    public bool ModelsHealthy { get; set; }

    /// <summary>Gets or sets latency in milliseconds.</summary>
    public int LatencyMs { get; set; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double? GpuUtilizationPercent { get; set; }

    /// <summary>Gets or sets memory used in bytes.</summary>
    public long? MemoryUsedBytes { get; set; }

    /// <summary>Gets or sets disk used in bytes.</summary>
    public long? DiskUsedBytes { get; set; }

    /// <summary>Gets or sets a value indicating whether the network is healthy.</summary>
    public bool NetworkHealthy { get; set; }

    /// <summary>Gets or sets the orchestration state.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; set; }
}
