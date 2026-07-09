namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Capacity planning response.
/// </summary>
public sealed class CapacityResponse
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the optional pool identifier.</summary>
    public Guid? PoolId { get; set; }

    /// <summary>Gets or sets current capacity (0-1).</summary>
    public double CurrentCapacity { get; set; }

    /// <summary>Gets or sets projected capacity (0-1).</summary>
    public double ProjectedCapacity { get; set; }

    /// <summary>Gets or sets remaining capacity (0-1).</summary>
    public double RemainingCapacity { get; set; }

    /// <summary>Gets or sets maximum throughput.</summary>
    public double MaximumThroughput { get; set; }

    /// <summary>Gets or sets suggested scale adjustment.</summary>
    public int SuggestedScale { get; set; }

    /// <summary>Gets or sets total pods.</summary>
    public int TotalPods { get; set; }

    /// <summary>Gets or sets healthy pods.</summary>
    public int HealthyPods { get; set; }

    /// <summary>Gets or sets busy pods.</summary>
    public int BusyPods { get; set; }

    /// <summary>Gets or sets queue length.</summary>
    public int QueueLength { get; set; }

    /// <summary>Gets or sets average wait time in milliseconds.</summary>
    public double AverageWaitTimeMs { get; set; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; set; }

    /// <summary>Gets or sets concurrent streams.</summary>
    public int ConcurrentStreams { get; set; }
}
