namespace PodPilot.Domain.Enums;

/// <summary>
/// Status of a gateway proxy request.
/// </summary>
public enum GatewayRequestStatus
{
    /// <summary>Request received.</summary>
    Pending = 0,

    /// <summary>Pod wake in progress.</summary>
    Waking = 1,

    /// <summary>Waiting for pod health.</summary>
    WaitingHealthy = 2,

    /// <summary>Forwarding to inference backend.</summary>
    Forwarding = 3,

    /// <summary>Streaming response.</summary>
    Streaming = 4,

    /// <summary>Request completed successfully.</summary>
    Completed = 5,

    /// <summary>Request failed.</summary>
    Failed = 6,

    /// <summary>Request was cancelled.</summary>
    Cancelled = 7,
}
