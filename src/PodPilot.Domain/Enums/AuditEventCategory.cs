namespace PodPilot.Domain.Enums;

/// <summary>
/// High-level category for enterprise audit events.
/// </summary>
public enum AuditEventCategory
{
    /// <summary>Authentication events.</summary>
    Authentication = 0,

    /// <summary>Authorization and permission changes.</summary>
    Authorization = 1,

    /// <summary>Organization lifecycle.</summary>
    Organization = 2,

    /// <summary>Provider configuration.</summary>
    Provider = 3,

    /// <summary>Pod lifecycle.</summary>
    Pod = 4,

    /// <summary>AI gateway / inference requests.</summary>
    AiRequest = 5,

    /// <summary>Plugin lifecycle.</summary>
    Plugin = 6,

    /// <summary>MCP registration and tools.</summary>
    Mcp = 7,

    /// <summary>Secret access and rotation.</summary>
    Secret = 8,

    /// <summary>Security policy changes.</summary>
    Policy = 9,

    /// <summary>Compliance actions.</summary>
    Compliance = 10,

    /// <summary>Security alerts.</summary>
    Security = 11,
}
