namespace PodPilot.Domain.Enums;

/// <summary>
/// Types of scheduler lifecycle events.
/// </summary>
public enum SchedulerEventType
{
    /// <summary>Request was queued.</summary>
    Queued = 0,

    /// <summary>Request was dispatched from the queue.</summary>
    Dispatched = 1,

    /// <summary>Request execution started.</summary>
    Started = 2,

    /// <summary>Request is streaming a response.</summary>
    Streaming = 3,

    /// <summary>Request completed successfully.</summary>
    Completed = 4,

    /// <summary>Request was cancelled.</summary>
    Cancelled = 5,

    /// <summary>Request was retried.</summary>
    Retried = 6,

    /// <summary>Request failed.</summary>
    Failed = 7,

    /// <summary>Request timed out.</summary>
    TimedOut = 8,

    /// <summary>Request was reassigned to another pod.</summary>
    Reassigned = 9,
}
