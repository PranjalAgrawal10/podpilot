namespace PodPilot.Domain.Entities;

/// <summary>
/// Discovered MCP tool definition.
/// </summary>
public class McpTool : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the MCP server identifier.</summary>
    public Guid McpServerId { get; set; }

    /// <summary>Gets or sets the tool name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the JSON schema for input.</summary>
    public string? InputSchemaJson { get; set; }

    /// <summary>Gets or sets whether the tool is enabled for execution.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets when the tool was last discovered.</summary>
    public DateTime DiscoveredAt { get; set; }

    /// <summary>Gets the MCP server.</summary>
    public McpServer McpServer { get; set; } = null!;
}
