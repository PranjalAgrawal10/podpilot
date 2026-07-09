namespace PodPilot.Domain.Enums;

/// <summary>
/// Trigger source for a scaling event.
/// </summary>
public enum ScalingTriggerType
{
    /// <summary>Automatic threshold-based scaling.</summary>
    Automatic = 0,

    /// <summary>Manual scaling via API.</summary>
    Manual = 1,

    /// <summary>Failover-driven scaling.</summary>
    Failover = 2,
}
