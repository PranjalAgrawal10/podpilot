namespace PodPilot.Domain.Enums;

/// <summary>
/// Health state of an AI inference provider.
/// </summary>
public enum AiProviderHealthState
{
    /// <summary>Provider has not been checked yet.</summary>
    Unknown = 0,

    /// <summary>Provider is healthy.</summary>
    Healthy = 1,

    /// <summary>Provider is degraded.</summary>
    Degraded = 2,

    /// <summary>Provider is unhealthy.</summary>
    Unhealthy = 3,
}
