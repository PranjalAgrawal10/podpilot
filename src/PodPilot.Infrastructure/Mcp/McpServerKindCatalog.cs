using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Mcp;

/// <summary>
/// Built-in MCP server kind metadata catalog.
/// </summary>
public sealed class McpServerKindCatalog : IMcpServerKindCatalog
{
    private static readonly IReadOnlyList<McpServerKindMetadata> Kinds =
    [
        Meta(McpServerKind.Custom, "Custom", "Any OpenMCP-compatible HTTP endpoint", null, "Bearer", false),
        Meta(McpServerKind.Filesystem, "Filesystem", "Local or remote filesystem tools", "http://127.0.0.1:7101/mcp", "None", false),
        Meta(McpServerKind.GitHub, "GitHub", "Repositories, issues, and PRs", "https://api.githubcopilot.com/mcp/", "Bearer", true),
        Meta(McpServerKind.GitLab, "GitLab", "Projects, MRs, and pipelines", null, "Bearer", true),
        Meta(McpServerKind.Docker, "Docker", "Containers and images", "http://127.0.0.1:7102/mcp", "None", false),
        Meta(McpServerKind.PostgreSQL, "PostgreSQL", "SQL tools and schema resources", null, "ApiKey", true),
        Meta(McpServerKind.MySQL, "MySQL", "SQL tools and schema resources", null, "ApiKey", true),
        Meta(McpServerKind.Redis, "Redis", "Cache and key browsing tools", null, "ApiKey", true),
        Meta(McpServerKind.Azure, "Azure", "Azure resource operations", null, "Bearer", true),
        Meta(McpServerKind.Aws, "AWS", "AWS resource operations", null, "ApiKey", true),
        Meta(McpServerKind.Kubernetes, "Kubernetes", "Cluster inspection tools", null, "Bearer", true),
        Meta(McpServerKind.Slack, "Slack", "Channels and messaging tools", null, "Bearer", true),
        Meta(McpServerKind.Discord, "Discord", "Guilds and messaging tools", null, "Bearer", true),
        Meta(McpServerKind.Jira, "Jira", "Issues and project tools", null, "Basic", true),
        Meta(McpServerKind.Confluence, "Confluence", "Knowledge base tools", null, "Basic", true),
        Meta(McpServerKind.Playwright, "Playwright", "Browser automation tools", "http://127.0.0.1:7103/mcp", "None", false),
        Meta(McpServerKind.Browser, "Browser", "Web browsing tools", "http://127.0.0.1:7104/mcp", "None", false),
        Meta(McpServerKind.Git, "Git", "Local git repository tools", "http://127.0.0.1:7105/mcp", "None", false),
        Meta(McpServerKind.Shell, "Shell", "Sandboxed shell execution tools", "http://127.0.0.1:7106/mcp", "None", false),
    ];

    /// <inheritdoc />
    public IReadOnlyList<McpServerKindMetadata> GetAll() => Kinds;

    /// <inheritdoc />
    public McpServerKindMetadata? Get(McpServerKind kind) =>
        Kinds.FirstOrDefault(k => k.ServerKind == kind);

    private static McpServerKindMetadata Meta(
        McpServerKind kind,
        string displayName,
        string description,
        string? endpoint,
        string auth,
        bool requiresCredential) =>
        new()
        {
            ServerKind = kind,
            DisplayName = displayName,
            Description = description,
            DefaultEndpoint = endpoint,
            DefaultAuthScheme = auth,
            RequiresCredential = requiresCredential,
        };
}
