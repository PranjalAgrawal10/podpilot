namespace PodPilot.Domain.Entities;

/// <summary>
/// Discovered MCP resource definition.
/// </summary>
public class McpResource : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the MCP server identifier.</summary>
    public Guid McpServerId { get; set; }

    /// <summary>Gets or sets the resource URI.</summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the MIME type.</summary>
    public string? MimeType { get; set; }

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets when the resource was last discovered.</summary>
    public DateTime DiscoveredAt { get; set; }

    /// <summary>Gets the MCP server.</summary>
    public McpServer McpServer { get; set; } = null!;
}
