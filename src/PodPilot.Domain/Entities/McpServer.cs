using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Organization-registered MCP server endpoint.
/// </summary>
public class McpServer : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the server version label.</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Gets or sets the MCP server kind.</summary>
    public McpServerKind ServerKind { get; set; } = McpServerKind.Custom;

    /// <summary>Gets or sets the endpoint URL.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Gets or sets the authentication scheme (None, Bearer, ApiKey, Basic).</summary>
    public string AuthScheme { get; set; } = "None";

    /// <summary>Gets or sets the encrypted credential payload when required.</summary>
    public string? EncryptedCredential { get; set; }

    /// <summary>Gets or sets the connection status.</summary>
    public McpConnectionStatus Status { get; set; } = McpConnectionStatus.Unknown;

    /// <summary>Gets or sets whether the server is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets the request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Gets or sets the max retry attempts.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Gets or sets whether streaming is supported/enabled.</summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>Gets or sets when the server was last checked.</summary>
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>Gets or sets the last error message.</summary>
    public string? LastError { get; set; }

    /// <summary>Gets discovered tools.</summary>
    public ICollection<McpTool> Tools { get; set; } = [];

    /// <summary>Gets discovered resources.</summary>
    public ICollection<McpResource> Resources { get; set; } = [];

    /// <summary>Gets discovered prompts.</summary>
    public ICollection<McpPrompt> Prompts { get; set; } = [];
}
