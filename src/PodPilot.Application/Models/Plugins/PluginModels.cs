using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Plugins;

/// <summary>
/// Runtime context passed to plugins (no secrets, org-scoped).
/// </summary>
public sealed class PluginContext
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the plugin installation identifier.</summary>
    public Guid InstallationId { get; init; }

    /// <summary>Gets or sets granted permission names.</summary>
    public IReadOnlyList<string> GrantedPermissions { get; init; } = [];

    /// <summary>Gets or sets non-secret configuration values.</summary>
    public IReadOnlyDictionary<string, string> Settings { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Result of a plugin health check.
/// </summary>
public sealed class PluginHealthResult
{
    /// <summary>Gets or sets a value indicating whether the plugin is healthy.</summary>
    public bool IsHealthy { get; init; }

    /// <summary>Gets or sets an optional message.</summary>
    public string? Message { get; init; }
}

/// <summary>
/// Descriptor for a discovered/local marketplace plugin package.
/// </summary>
public sealed class PluginPackageDescriptor
{
    /// <summary>Gets or sets the package identifier.</summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the version.</summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>Gets or sets the plugin type.</summary>
    public PluginType PluginType { get; init; }

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the publisher.</summary>
    public string Publisher { get; init; } = "PodPilot";

    /// <summary>Gets or sets whether the package is first-party.</summary>
    public bool IsFirstParty { get; init; } = true;

    /// <summary>Gets or sets required permission names.</summary>
    public IReadOnlyList<string> RequiredPermissions { get; init; } = [];

    /// <summary>Gets or sets optional entry assembly name.</summary>
    public string? EntryAssembly { get; init; }

    /// <summary>Gets or sets optional entry type name.</summary>
    public string? EntryType { get; init; }

    /// <summary>Gets or sets optional settings schema JSON.</summary>
    public string? SettingsSchemaJson { get; init; }
}

/// <summary>
/// MCP tool call request.
/// </summary>
public sealed class McpToolCallRequest
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the server identifier.</summary>
    public Guid ServerId { get; init; }

    /// <summary>Gets or sets the tool name.</summary>
    public string ToolName { get; init; } = string.Empty;

    /// <summary>Gets or sets arguments as JSON.</summary>
    public string ArgumentsJson { get; init; } = "{}";
}

/// <summary>
/// MCP tool call result.
/// </summary>
public sealed class McpToolCallResult
{
    /// <summary>Gets or sets a value indicating success.</summary>
    public bool Succeeded { get; init; }

    /// <summary>Gets or sets the content JSON payload.</summary>
    public string ContentJson { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets duration in milliseconds.</summary>
    public int DurationMs { get; init; }
}

/// <summary>
/// Metadata for a built-in MCP server kind.
/// </summary>
public sealed class McpServerKindMetadata
{
    /// <summary>Gets or sets the server kind.</summary>
    public McpServerKind ServerKind { get; init; }

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets a short description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets or sets the suggested default endpoint.</summary>
    public string? DefaultEndpoint { get; init; }

    /// <summary>Gets or sets the recommended auth scheme.</summary>
    public string DefaultAuthScheme { get; init; } = "None";

    /// <summary>Gets or sets whether a credential is required.</summary>
    public bool RequiresCredential { get; init; }
}

/// <summary>
/// Discovered MCP tool info.
/// </summary>
public sealed class McpToolInfo
{
    /// <summary>Gets or sets the tool name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the input schema JSON.</summary>
    public string? InputSchemaJson { get; init; }
}

/// <summary>
/// Discovered MCP resource info.
/// </summary>
public sealed class McpResourceInfo
{
    /// <summary>Gets or sets the URI.</summary>
    public string Uri { get; init; } = string.Empty;

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the MIME type.</summary>
    public string? MimeType { get; init; }

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Discovered MCP prompt info.
/// </summary>
public sealed class McpPromptInfo
{
    /// <summary>Gets or sets the prompt name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets arguments JSON.</summary>
    public string? ArgumentsJson { get; init; }
}

/// <summary>
/// Plugin dashboard summary.
/// </summary>
public sealed class PluginDashboard
{
    /// <summary>Gets or sets installed plugin count.</summary>
    public int InstalledPlugins { get; init; }

    /// <summary>Gets or sets enabled plugin count.</summary>
    public int EnabledPlugins { get; init; }

    /// <summary>Gets or sets connected MCP server count.</summary>
    public int ConnectedMcpServers { get; init; }

    /// <summary>Gets or sets available tool count.</summary>
    public int AvailableTools { get; init; }

    /// <summary>Gets or sets unhealthy plugin count.</summary>
    public int UnhealthyPlugins { get; init; }

    /// <summary>Gets or sets recent tool execution count.</summary>
    public int RecentExecutions { get; init; }
}
