namespace PodPilot.Domain.Enums;

/// <summary>
/// Types of activity recorded against a GPU pod.
/// </summary>
public enum PodActivityType
{
    /// <summary>Manual user interaction.</summary>
    Manual = 0,

    /// <summary>API request to the pod.</summary>
    ApiRequest = 1,

    /// <summary>Health check probe.</summary>
    HealthCheck = 2,

    /// <summary>Idle state detected by the lifecycle engine.</summary>
    IdleDetected = 3,

    /// <summary>Wake requested.</summary>
    WakeRequested = 4,

    /// <summary>Shutdown requested.</summary>
    ShutdownRequested = 5,

    /// <summary>Lifecycle policy updated.</summary>
    PolicyChange = 6,

    /// <summary>Pod started.</summary>
    Started = 7,

    /// <summary>Pod stopped.</summary>
    Stopped = 8,
}
