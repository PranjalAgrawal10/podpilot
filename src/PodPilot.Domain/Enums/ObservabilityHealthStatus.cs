namespace PodPilot.Domain.Enums;

/// <summary>
/// Health status for observability monitoring.
/// </summary>
public enum ObservabilityHealthStatus
{
    /// <summary>Component is healthy.</summary>
    Healthy = 0,

    /// <summary>Component is degraded but operational.</summary>
    Degraded = 1,

    /// <summary>Component is unhealthy.</summary>
    Unhealthy = 2,
}
