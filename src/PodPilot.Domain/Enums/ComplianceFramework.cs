namespace PodPilot.Domain.Enums;

/// <summary>
/// Compliance frameworks tracked by PodPilot.
/// </summary>
public enum ComplianceFramework
{
    /// <summary>General Data Protection Regulation.</summary>
    Gdpr = 0,

    /// <summary>SOC 2 readiness tracking.</summary>
    Soc2 = 1,

    /// <summary>ISO 27001 readiness tracking.</summary>
    Iso27001 = 2,
}
