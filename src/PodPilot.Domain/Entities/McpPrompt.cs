namespace PodPilot.Domain.Entities;

/// <summary>
/// Discovered MCP prompt template.
/// </summary>
public class McpPrompt : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the MCP server identifier.</summary>
    public Guid McpServerId { get; set; }

    /// <summary>Gets or sets the prompt name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets argument schema JSON.</summary>
    public string? ArgumentsJson { get; set; }

    /// <summary>Gets or sets when the prompt was last discovered.</summary>
    public DateTime DiscoveredAt { get; set; }

    /// <summary>Gets the MCP server.</summary>
    public McpServer McpServer { get; set; } = null!;
}
