namespace PodPilot.Domain.Enums;

/// <summary>
/// Connection health status for an MCP server.
/// </summary>
public enum McpConnectionStatus
{
    /// <summary>Never connected or unknown.</summary>
    Unknown = 0,

    /// <summary>Successfully connected.</summary>
    Connected = 1,

    /// <summary>Disconnected or unreachable.</summary>
    Disconnected = 2,

    /// <summary>Connected but degraded.</summary>
    Degraded = 3,

    /// <summary>Authentication failed.</summary>
    Unauthorized = 4,
}
