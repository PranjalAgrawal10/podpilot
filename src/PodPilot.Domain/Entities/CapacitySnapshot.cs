namespace PodPilot.Domain.Entities;

/// <summary>
/// Point-in-time capacity snapshot for an organization or pool.
/// </summary>
public class CapacitySnapshot : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the pod pool identifier.
    /// </summary>
    public Guid? PodPoolId { get; set; }

    /// <summary>
    /// Gets or sets when the snapshot was recorded.
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Gets or sets the total pod count.
    /// </summary>
    public int TotalPods { get; set; }

    /// <summary>
    /// Gets or sets the healthy pod count.
    /// </summary>
    public int HealthyPods { get; set; }

    /// <summary>
    /// Gets or sets the busy pod count.
    /// </summary>
    public int BusyPods { get; set; }

    /// <summary>
    /// Gets or sets the current queue length.
    /// </summary>
    public int QueueLength { get; set; }

    /// <summary>
    /// Gets or sets the average wait time in milliseconds.
    /// </summary>
    public double AverageWaitTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the average latency in milliseconds.
    /// </summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets the GPU utilization percentage.
    /// </summary>
    public double GpuUtilizationPercent { get; set; }

    /// <summary>
    /// Gets or sets the concurrent stream count.
    /// </summary>
    public int ConcurrentStreams { get; set; }

    /// <summary>
    /// Gets or sets the current capacity (0-1).
    /// </summary>
    public double CurrentCapacity { get; set; }

    /// <summary>
    /// Gets or sets the projected capacity (0-1).
    /// </summary>
    public double ProjectedCapacity { get; set; }

    /// <summary>
    /// Gets or sets the remaining capacity (0-1).
    /// </summary>
    public double RemainingCapacity { get; set; }

    /// <summary>
    /// Gets or sets the maximum throughput (requests per second).
    /// </summary>
    public double MaximumThroughput { get; set; }

    /// <summary>
    /// Gets or sets the suggested scale adjustment.
    /// </summary>
    public int SuggestedScale { get; set; }
}
