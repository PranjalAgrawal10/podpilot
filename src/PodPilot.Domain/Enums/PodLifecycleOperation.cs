namespace PodPilot.Domain.Enums;

/// <summary>
/// Lifecycle operations that require distributed locking.
/// </summary>
public enum PodLifecycleOperation
{
    /// <summary>Wake a stopped pod.</summary>
    Wake = 0,

    /// <summary>Shut down a running pod.</summary>
    Shutdown = 1,

    /// <summary>Replace a failed provider pod instance.</summary>
    Replacement = 2,
}
