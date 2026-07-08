namespace PodPilot.Domain.Enums;

/// <summary>
/// Scheduler-specific request status values.
/// </summary>
public enum SchedulerRequestStatus
{
    /// <summary>Request is queued.</summary>
    Queued = 0,

    /// <summary>Request is waiting for a pod.</summary>
    WaitingForPod = 1,

    /// <summary>Pod is starting.</summary>
    StartingPod = 2,

    /// <summary>Request is running.</summary>
    Running = 3,

    /// <summary>Request is streaming.</summary>
    Streaming = 4,

    /// <summary>Request completed.</summary>
    Completed = 5,

    /// <summary>Request was cancelled.</summary>
    Cancelled = 6,

    /// <summary>Request failed.</summary>
    Failed = 7,

    /// <summary>Request timed out.</summary>
    TimedOut = 8,
}
