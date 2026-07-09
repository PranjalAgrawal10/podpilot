namespace PodPilot.Domain.Enums;

/// <summary>
/// Alert severity levels.
/// </summary>
public enum AlertSeverity
{
    /// <summary>Informational alert.</summary>
    Info = 0,

    /// <summary>Warning requiring attention.</summary>
    Warning = 1,

    /// <summary>Critical alert requiring immediate action.</summary>
    Critical = 2,
}
