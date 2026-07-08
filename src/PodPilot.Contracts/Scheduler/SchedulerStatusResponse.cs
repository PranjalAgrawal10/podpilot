namespace PodPilot.Contracts.Scheduler;

/// <summary>
/// Scheduler health and metrics response.
/// </summary>
public sealed class SchedulerStatusResponse
{
    /// <summary>Gets or sets whether the scheduler is healthy.</summary>
    public bool IsHealthy { get; init; }

    /// <summary>Gets or sets active tracked requests.</summary>
    public int ActiveTrackedRequests { get; init; }

    /// <summary>Gets or sets total queued requests.</summary>
    public int TotalQueuedRequests { get; init; }

    /// <summary>Gets or sets total running requests.</summary>
    public int TotalRunningRequests { get; init; }

    /// <summary>Gets or sets retry count in the last hour.</summary>
    public int RetriesLastHour { get; init; }

    /// <summary>Gets or sets pod utilization percentage.</summary>
    public double PodUtilizationPercent { get; init; }
}
