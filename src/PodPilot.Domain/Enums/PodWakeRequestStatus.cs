namespace PodPilot.Domain.Enums;

/// <summary>
/// Status of a queued pod wake request.
/// </summary>
public enum PodWakeRequestStatus
{
    /// <summary>Waiting to be processed.</summary>
    Pending = 0,

    /// <summary>Currently being processed.</summary>
    Processing = 1,

    /// <summary>Wake completed successfully.</summary>
    Completed = 2,

    /// <summary>Wake failed.</summary>
    Failed = 3,

    /// <summary>Wake request cancelled.</summary>
    Cancelled = 4,
}
