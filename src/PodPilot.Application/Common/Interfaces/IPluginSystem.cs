using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Contract implemented by every plugin package. Plugins must only communicate via this surface.
/// </summary>
public interface IPlugin
{
    /// <summary>Gets the unique package identifier.</summary>
    string PackageId { get; }

    /// <summary>Gets the display name.</summary>
    string Name { get; }

    /// <summary>Gets the semantic version.</summary>
    string Version { get; }

    /// <summary>Gets the plugin type.</summary>
    PluginType PluginType { get; }

    /// <summary>Gets required permission names.</summary>
    IReadOnlyList<string> RequiredPermissions { get; }

    /// <summary>Initializes the plugin for an organization installation.</summary>
    Task InitializeAsync(PluginContext context, CancellationToken cancellationToken = default);

    /// <summary>Starts the plugin after enable.</summary>
    Task StartAsync(PluginContext context, CancellationToken cancellationToken = default);

    /// <summary>Stops the plugin on disable/uninstall.</summary>
    Task StopAsync(PluginContext context, CancellationToken cancellationToken = default);

    /// <summary>Performs a health check.</summary>
    Task<PluginHealthResult> CheckHealthAsync(PluginContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Discovers and loads plugin assemblies (hot-load where possible).
/// </summary>
public interface IPluginLoader
{
    /// <summary>Loads plugins from registered first-party implementations and optional disk packages.</summary>
    Task<IReadOnlyList<IPlugin>> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory registry of loaded plugin instances.
/// </summary>
public interface IPluginRegistry
{
    /// <summary>Registers a loaded plugin instance.</summary>
    void Register(IPlugin plugin);

    /// <summary>Unregisters a plugin by package id.</summary>
    bool Unregister(string packageId);

    /// <summary>Gets a plugin by package id.</summary>
    IPlugin? Get(string packageId);

    /// <summary>Lists registered plugins.</summary>
    IReadOnlyList<IPlugin> List();
}

/// <summary>
/// Installs and uninstalls plugins for an organization.
/// </summary>
public interface IPluginInstaller
{
    /// <summary>Ensures catalog definitions exist for available packages.</summary>
    Task SyncCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>Installs a plugin package into an organization.</summary>
    Task<Guid> InstallAsync(
        Guid organizationId,
        string packageId,
        IReadOnlyList<string>? grantedPermissions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Uninstalls a plugin installation.</summary>
    Task UninstallAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Manages plugin enable/disable lifecycle and health for an organization.
/// </summary>
public interface IPluginManager
{
    /// <summary>Enables a plugin installation.</summary>
    Task EnableAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default);

    /// <summary>Disables a plugin installation.</summary>
    Task DisableAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default);

    /// <summary>Updates settings for an installation.</summary>
    Task UpdateSettingsAsync(
        Guid organizationId,
        Guid installationId,
        IReadOnlyDictionary<string, string> settings,
        IReadOnlySet<string> secretKeys,
        CancellationToken cancellationToken = default);

    /// <summary>Runs health checks for enabled plugins.</summary>
    Task CheckHealthAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Gets dashboard metrics.</summary>
    Task<PluginDashboard> GetDashboardAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Low-level MCP connection (JSON-RPC) to a single server endpoint.
/// </summary>
public interface IMcpConnection : IAsyncDisposable
{
    /// <summary>Gets a value indicating whether the connection is open.</summary>
    bool IsConnected { get; }

    /// <summary>Initializes the MCP session.</summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists tools.</summary>
    Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists resources.</summary>
    Task<IReadOnlyList<McpResourceInfo>> ListResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists prompts.</summary>
    Task<IReadOnlyList<McpPromptInfo>> ListPromptsAsync(CancellationToken cancellationToken = default);

    /// <summary>Calls a tool.</summary>
    Task<McpToolCallResult> CallToolAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken = default);

    /// <summary>Performs a lightweight health ping.</summary>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory for MCP connections.
/// </summary>
public interface IMcpConnectionFactory
{
    /// <summary>Creates a connection for a persisted server configuration.</summary>
    Task<IMcpConnection> CreateAsync(
        Domain.Entities.McpServer server,
        string? decryptedCredential,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Registry of MCP servers and discovered capabilities for an organization.
/// </summary>
public interface IMcpRegistry
{
    /// <summary>Refreshes tools/resources/prompts for a server.</summary>
    Task DiscoverAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default);

    /// <summary>Lists built-in kind metadata.</summary>
    IReadOnlyList<McpServerKindMetadata> ListKinds();

    /// <summary>Resolves a tool by name across enabled servers.</summary>
    Task<(Domain.Entities.McpServer Server, Domain.Entities.McpTool Tool)?> ResolveToolAsync(
        Guid organizationId,
        string toolName,
        Guid? preferredServerId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Proxies AI tool calls to MCP servers with retry/timeout.
/// </summary>
public interface IMcpProxy
{
    /// <summary>Executes a tool call through the MCP registry.</summary>
    Task<McpToolCallResult> ExecuteToolAsync(
        McpToolCallRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// MCP server kind metadata catalog (built-ins).
/// </summary>
public interface IMcpServerKindCatalog
{
    /// <summary>Gets metadata for all supported kinds.</summary>
    IReadOnlyList<McpServerKindMetadata> GetAll();

    /// <summary>Gets metadata for a kind.</summary>
    McpServerKindMetadata? Get(McpServerKind kind);
}

/// <summary>
/// SignalR notifications for plugins and MCP.
/// </summary>
public interface IPluginNotificationService
{
    /// <summary>Notifies that a plugin was installed.</summary>
    Task NotifyPluginInstalledAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a plugin was removed.</summary>
    Task NotifyPluginRemovedAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a plugin was updated.</summary>
    Task NotifyPluginUpdatedAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that an MCP server connected.</summary>
    Task NotifyMcpConnectedAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that an MCP server disconnected.</summary>
    Task NotifyMcpDisconnectedAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a tool was executed.</summary>
    Task NotifyToolExecutedAsync(
        Guid organizationId,
        Guid serverId,
        string toolName,
        bool succeeded,
        CancellationToken cancellationToken = default);
}
