namespace PodPilot.Domain.Enums;

/// <summary>
/// Built-in and custom MCP server kinds.
/// </summary>
public enum McpServerKind
{
    /// <summary>Custom / third-party MCP endpoint.</summary>
    Custom = 0,

    /// <summary>Local or remote filesystem MCP server.</summary>
    Filesystem = 1,

    /// <summary>GitHub MCP server.</summary>
    GitHub = 2,

    /// <summary>GitLab MCP server.</summary>
    GitLab = 3,

    /// <summary>Docker MCP server.</summary>
    Docker = 4,

    /// <summary>PostgreSQL MCP server.</summary>
    PostgreSQL = 5,

    /// <summary>MySQL MCP server.</summary>
    MySQL = 6,

    /// <summary>Redis MCP server.</summary>
    Redis = 7,

    /// <summary>Azure MCP server.</summary>
    Azure = 8,

    /// <summary>AWS MCP server.</summary>
    Aws = 9,

    /// <summary>Kubernetes MCP server.</summary>
    Kubernetes = 10,

    /// <summary>Slack MCP server.</summary>
    Slack = 11,

    /// <summary>Discord MCP server.</summary>
    Discord = 12,

    /// <summary>Jira MCP server.</summary>
    Jira = 13,

    /// <summary>Confluence MCP server.</summary>
    Confluence = 14,

    /// <summary>Playwright browser automation MCP server.</summary>
    Playwright = 15,

    /// <summary>Generic browser MCP server.</summary>
    Browser = 16,

    /// <summary>Git VCS MCP server.</summary>
    Git = 17,

    /// <summary>Shell / command execution MCP server.</summary>
    Shell = 18,
}
