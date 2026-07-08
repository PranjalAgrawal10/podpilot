namespace PodPilot.Domain.Enums;

/// <summary>
/// Priority level for scheduled gateway requests.
/// </summary>
public enum RequestPriority
{
    /// <summary>Lowest priority background work.</summary>
    Background = 0,

    /// <summary>Standard batch or deferred work.</summary>
    Low = 1,

    /// <summary>Default interactive traffic.</summary>
    Normal = 2,

    /// <summary>User-facing interactive requests.</summary>
    High = 3,

    /// <summary>Administrative or emergency traffic.</summary>
    Critical = 4,
}
