using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Plugins;

/// <summary>
/// Broadcasts plugin/MCP events via SignalR.
/// </summary>
public sealed class PluginNotificationService : IPluginNotificationService
{
    private readonly IHubContext<PluginHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginNotificationService"/> class.
    /// </summary>
    public PluginNotificationService(IHubContext<PluginHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyPluginInstalledAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PluginInstalled", new { installationId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyPluginRemovedAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PluginRemoved", new { installationId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyPluginUpdatedAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PluginUpdated", new { installationId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyMcpConnectedAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "McpConnected", new { serverId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyMcpDisconnectedAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "McpDisconnected", new { serverId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyToolExecutedAsync(
        Guid organizationId,
        Guid serverId,
        string toolName,
        bool succeeded,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "ToolExecuted", new { serverId, toolName, succeeded }, cancellationToken);

    private Task SendAsync(Guid organizationId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(PluginHub.GetOrganizationGroupName(organizationId))
            .SendAsync(method, payload, cancellationToken);
}

/// <summary>
/// No-op notifications for Testing.
/// </summary>
public sealed class NoOpPluginNotificationService : IPluginNotificationService
{
    /// <inheritdoc />
    public Task NotifyPluginInstalledAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPluginRemovedAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPluginUpdatedAsync(Guid organizationId, Guid installationId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyMcpConnectedAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyMcpDisconnectedAsync(Guid organizationId, Guid serverId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyToolExecutedAsync(
        Guid organizationId,
        Guid serverId,
        string toolName,
        bool succeeded,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
