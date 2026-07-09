using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Point-in-time health metrics for a GPU pod.
/// </summary>
public class PodHealthMetric : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets when metrics were recorded.
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the GPU is healthy.
    /// </summary>
    public bool GpuHealthy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Ollama is healthy.
    /// </summary>
    public bool OllamaHealthy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether models are healthy.
    /// </summary>
    public bool ModelsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the measured latency in milliseconds.
    /// </summary>
    public int LatencyMs { get; set; }

    /// <summary>
    /// Gets or sets GPU utilization percentage.
    /// </summary>
    public double? GpuUtilizationPercent { get; set; }

    /// <summary>
    /// Gets or sets memory used in bytes.
    /// </summary>
    public long? MemoryUsedBytes { get; set; }

    /// <summary>
    /// Gets or sets disk used in bytes.
    /// </summary>
    public long? DiskUsedBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the network is healthy.
    /// </summary>
    public bool NetworkHealthy { get; set; }

    /// <summary>
    /// Gets or sets the orchestration state at recording time.
    /// </summary>
    public OrchestrationPodState State { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
