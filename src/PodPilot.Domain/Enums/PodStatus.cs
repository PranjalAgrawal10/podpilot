namespace PodPilot.Domain.Enums;

/// <summary>
/// Lifecycle status of a GPU pod.
/// </summary>
public enum PodStatus
{
    /// <summary>Pod is being provisioned.</summary>
    Creating = 0,

    /// <summary>Pod is building or staging on the provider.</summary>
    BuildingPending = 10,

    /// <summary>Pod is starting.</summary>
    Starting = 1,

    /// <summary>Pod is running.</summary>
    Running = 2,

    /// <summary>Pod is stopping.</summary>
    Stopping = 3,

    /// <summary>Pod is stopped.</summary>
    Stopped = 4,

    /// <summary>Pod is restarting.</summary>
    Restarting = 5,

    /// <summary>Pod is being deleted.</summary>
    Deleting = 6,

    /// <summary>Pod has been deleted.</summary>
    Deleted = 7,

    /// <summary>Pod failed to provision or operate.</summary>
    Failed = 8,

    /// <summary>Status is unknown or pending sync.</summary>
    Unknown = 9,
}
