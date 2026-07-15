namespace PodPilot.Domain.Enums;

/// <summary>
/// Extensible plugin type. New types can be registered without core schema changes when stored as string names.
/// </summary>
public enum PluginType
{
    /// <summary>AI inference provider integration.</summary>
    AiProvider = 0,

    /// <summary>Object or file storage.</summary>
    Storage = 1,

    /// <summary>Notification channels.</summary>
    Notification = 2,

    /// <summary>Authentication / identity providers.</summary>
    Authentication = 3,

    /// <summary>Monitoring and telemetry.</summary>
    Monitoring = 4,

    /// <summary>Database connectors.</summary>
    Database = 5,

    /// <summary>Developer tooling.</summary>
    DeveloperTool = 6,

    /// <summary>General utilities.</summary>
    Utility = 7,

    /// <summary>MCP server bridge.</summary>
    McpBridge = 8,
}
