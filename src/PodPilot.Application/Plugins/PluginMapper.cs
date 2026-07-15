using System.Text.Json;
using PodPilot.Contracts.Plugins;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Plugins;

/// <summary>
/// Maps plugin/MCP entities to contracts.
/// </summary>
internal static class PluginMapper
{
    public static PluginResponse ToCatalogResponse(PluginDefinition definition, PluginInstallation? installation = null) =>
        new()
        {
            Id = definition.Id,
            InstallationId = installation?.Id,
            PackageId = definition.PackageId,
            Name = definition.Name,
            Version = definition.Version,
            PluginType = definition.PluginType.ToString(),
            Description = definition.Description,
            Publisher = definition.Publisher,
            IsFirstParty = definition.IsFirstParty,
            Status = installation?.Status.ToString(),
            IsHealthy = installation?.IsHealthy,
            HealthMessage = installation?.HealthMessage,
            RequiredPermissions = ParseStrings(definition.RequiredPermissionsJson),
            GrantedPermissions = installation is null ? [] : ParseStrings(installation.GrantedPermissionsJson),
            EnabledAt = installation?.EnabledAt,
            CreatedAt = installation?.CreatedAt ?? definition.CreatedAt,
        };

    public static McpServerResponse ToMcpServerResponse(McpServer server) =>
        new()
        {
            Id = server.Id,
            OrganizationId = server.OrganizationId,
            Name = server.Name,
            Version = server.Version,
            ServerKind = server.ServerKind.ToString(),
            Endpoint = server.Endpoint,
            AuthScheme = server.AuthScheme,
            HasCredential = !string.IsNullOrWhiteSpace(server.EncryptedCredential),
            Status = server.Status.ToString(),
            IsEnabled = server.IsEnabled,
            TimeoutSeconds = server.TimeoutSeconds,
            MaxRetries = server.MaxRetries,
            SupportsStreaming = server.SupportsStreaming,
            LastCheckedAt = server.LastCheckedAt,
            LastError = server.LastError,
            ToolCount = server.Tools?.Count ?? 0,
            ResourceCount = server.Resources?.Count ?? 0,
            CreatedAt = server.CreatedAt,
        };

    public static McpToolResponse ToMcpToolResponse(McpTool tool) =>
        new()
        {
            Id = tool.Id,
            McpServerId = tool.McpServerId,
            ServerName = tool.McpServer?.Name ?? string.Empty,
            Name = tool.Name,
            Description = tool.Description,
            InputSchemaJson = tool.InputSchemaJson,
            IsEnabled = tool.IsEnabled,
            DiscoveredAt = tool.DiscoveredAt,
        };

    public static McpResourceResponse ToMcpResourceResponse(McpResource resource) =>
        new()
        {
            Id = resource.Id,
            McpServerId = resource.McpServerId,
            ServerName = resource.McpServer?.Name ?? string.Empty,
            Uri = resource.Uri,
            Name = resource.Name,
            MimeType = resource.MimeType,
            Description = resource.Description,
            DiscoveredAt = resource.DiscoveredAt,
        };

    private static IReadOnlyList<string> ParseStrings(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
