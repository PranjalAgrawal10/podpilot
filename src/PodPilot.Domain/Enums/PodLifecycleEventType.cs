namespace PodPilot.Domain.Enums;

/// <summary>
/// Lifecycle events emitted by the auto wake/shutdown engine.
/// </summary>
public enum PodLifecycleEventType
{
    /// <summary>Wake has been requested.</summary>
    WakeRequested = 0,

    /// <summary>Wake operation started.</summary>
    WakeStarted = 1,

    /// <summary>Wake operation completed.</summary>
    WakeCompleted = 2,

    /// <summary>Shutdown has been requested.</summary>
    ShutdownRequested = 3,

    /// <summary>Shutdown operation started.</summary>
    ShutdownStarted = 4,

    /// <summary>Shutdown operation completed.</summary>
    ShutdownCompleted = 5,

    /// <summary>Pod detected as idle.</summary>
    IdleDetected = 6,

    /// <summary>Idle policy updated.</summary>
    PolicyUpdated = 7,

    /// <summary>Pod entered sleeping state.</summary>
    PodSleeping = 8,

    /// <summary>Pod is waking up.</summary>
    PodWaking = 9,

    /// <summary>Pod started running.</summary>
    PodStarted = 10,

    /// <summary>Pod stopped.</summary>
    PodStopped = 11,
}
