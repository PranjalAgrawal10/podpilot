namespace PodPilot.Contracts.Plugins;

/// <summary>MCP server response.</summary>
public sealed class McpServerResponse
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string ServerKind { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string AuthScheme { get; init; } = string.Empty;
    public bool HasCredential { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public int TimeoutSeconds { get; init; }
    public int MaxRetries { get; init; }
    public bool SupportsStreaming { get; init; }
    public DateTime? LastCheckedAt { get; init; }
    public string? LastError { get; init; }
    public int ToolCount { get; init; }
    public int ResourceCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Create MCP server request.</summary>
public sealed class CreateMcpServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string ServerKind { get; set; } = "Custom";
    public string Endpoint { get; set; } = string.Empty;
    public string AuthScheme { get; set; } = "None";
    public string? Credential { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 2;
    public bool SupportsStreaming { get; set; } = true;
    public bool DiscoverOnCreate { get; set; } = true;
}

/// <summary>MCP tool response.</summary>
public sealed class McpToolResponse
{
    public Guid Id { get; init; }
    public Guid McpServerId { get; init; }
    public string ServerName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? InputSchemaJson { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime DiscoveredAt { get; init; }
}

/// <summary>MCP resource response.</summary>
public sealed class McpResourceResponse
{
    public Guid Id { get; init; }
    public Guid McpServerId { get; init; }
    public string ServerName { get; init; } = string.Empty;
    public string Uri { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? MimeType { get; init; }
    public string? Description { get; init; }
    public DateTime DiscoveredAt { get; init; }
}

/// <summary>MCP server kind metadata.</summary>
public sealed class McpServerKindResponse
{
    public string ServerKind { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? DefaultEndpoint { get; init; }
    public string DefaultAuthScheme { get; init; } = "None";
    public bool RequiresCredential { get; init; }
}

/// <summary>Execute MCP tool request.</summary>
public sealed class ExecuteMcpToolRequest
{
    public Guid? ServerId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = "{}";
}

/// <summary>Execute MCP tool response.</summary>
public sealed class ExecuteMcpToolResponse
{
    public bool Succeeded { get; init; }
    public string ContentJson { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public int DurationMs { get; init; }
}
