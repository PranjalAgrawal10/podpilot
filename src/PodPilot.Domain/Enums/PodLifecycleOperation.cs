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
}
