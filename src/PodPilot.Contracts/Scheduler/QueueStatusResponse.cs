namespace PodPilot.Contracts.Scheduler;

/// <summary>
/// Queue status response.
/// </summary>
public sealed class QueueStatusResponse
{
    /// <summary>Gets or sets the queue length.</summary>
    public int QueueLength { get; init; }

    /// <summary>Gets or sets running request count.</summary>
    public int RunningRequests { get; init; }

    /// <summary>Gets or sets streaming request count.</summary>
    public int StreamingRequests { get; init; }

    /// <summary>Gets or sets failed request count in the last hour.</summary>
    public int FailedRequestsLastHour { get; init; }

    /// <summary>Gets or sets average wait time in milliseconds.</summary>
    public double AverageWaitTimeMs { get; init; }

    /// <summary>Gets or sets average execution time in milliseconds.</summary>
    public double AverageExecutionTimeMs { get; init; }
}
