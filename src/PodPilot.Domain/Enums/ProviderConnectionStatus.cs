namespace PodPilot.Domain.Enums;

/// <summary>
/// Connection health status for a compute provider.
/// </summary>
public enum ProviderConnectionStatus
{
    /// <summary>Provider is reachable and credentials are valid.</summary>
    Connected = 0,

    /// <summary>Provider is unreachable or credentials are invalid.</summary>
    Disconnected = 1,

    /// <summary>Provider is partially reachable or degraded.</summary>
    Degraded = 2,

    /// <summary>Health has not been checked yet.</summary>
    Unknown = 3,
}
