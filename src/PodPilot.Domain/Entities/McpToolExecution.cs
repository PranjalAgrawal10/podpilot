namespace PodPilot.Domain.Entities;

/// <summary>
/// Record of an MCP tool invocation.
/// </summary>
public class McpToolExecution : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the MCP server identifier.</summary>
    public Guid McpServerId { get; set; }

    /// <summary>Gets or sets the tool name.</summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating success.</summary>
    public bool Succeeded { get; set; }

    /// <summary>Gets or sets duration in milliseconds.</summary>
    public int DurationMs { get; set; }

    /// <summary>Gets or sets an optional error message (never includes secrets).</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets when the execution completed.</summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>Gets the MCP server.</summary>
    public McpServer McpServer { get; set; } = null!;
}
