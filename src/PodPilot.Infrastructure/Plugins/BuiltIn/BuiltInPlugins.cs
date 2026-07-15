using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Plugins.BuiltIn;

/// <summary>Base helper for first-party plugins.</summary>
public abstract class BuiltInPluginBase : IPlugin
{
    /// <inheritdoc />
    public abstract string PackageId { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public virtual string Version => "1.0.0";

    /// <inheritdoc />
    public abstract PluginType PluginType { get; }

    /// <inheritdoc />
    public virtual IReadOnlyList<string> RequiredPermissions { get; } = [];

    /// <inheritdoc />
    public virtual Task InitializeAsync(PluginContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task StartAsync(PluginContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task StopAsync(PluginContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task<PluginHealthResult> CheckHealthAsync(
        PluginContext context,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PluginHealthResult { IsHealthy = true, Message = "OK" });
}

/// <summary>Utility plugin that exposes sandbox health.</summary>
public sealed class UtilityHealthPlugin : BuiltInPluginBase
{
    /// <inheritdoc />
    public override string PackageId => "com.podpilot.utility.health";

    /// <inheritdoc />
    public override string Name => "Platform Health Utility";

    /// <inheritdoc />
    public override PluginType PluginType => PluginType.Utility;

    /// <inheritdoc />
    public override IReadOnlyList<string> RequiredPermissions { get; } = ["Plugin.Read"];
}

/// <summary>Notification plugin stub that validates configuration presence.</summary>
public sealed class NotificationAuditPlugin : BuiltInPluginBase
{
    /// <inheritdoc />
    public override string PackageId => "com.podpilot.notification.audit";

    /// <inheritdoc />
    public override string Name => "Audit Notification Channel";

    /// <inheritdoc />
    public override PluginType PluginType => PluginType.Notification;

    /// <inheritdoc />
    public override IReadOnlyList<string> RequiredPermissions { get; } = ["Plugin.Manage"];

    /// <inheritdoc />
    public override Task<PluginHealthResult> CheckHealthAsync(
        PluginContext context,
        CancellationToken cancellationToken = default)
    {
        var hasWebhook = context.Settings.ContainsKey("webhookUrl");
        return Task.FromResult(new PluginHealthResult
        {
            IsHealthy = true,
            Message = hasWebhook ? "Webhook configured" : "Using audit log sink",
        });
    }
}

/// <summary>Developer tool plugin for MCP bridging metadata.</summary>
public sealed class McpBridgePlugin : BuiltInPluginBase
{
    /// <inheritdoc />
    public override string PackageId => "com.podpilot.mcp.bridge";

    /// <inheritdoc />
    public override string Name => "MCP Tool Bridge";

    /// <inheritdoc />
    public override PluginType PluginType => PluginType.McpBridge;

    /// <inheritdoc />
    public override IReadOnlyList<string> RequiredPermissions { get; } = ["Mcp.Read", "Mcp.Manage"];
}

/// <summary>Monitoring plugin for plugin execution telemetry hooks.</summary>
public sealed class MonitoringTelemetryPlugin : BuiltInPluginBase
{
    /// <inheritdoc />
    public override string PackageId => "com.podpilot.monitoring.telemetry";

    /// <inheritdoc />
    public override string Name => "Plugin Telemetry";

    /// <inheritdoc />
    public override PluginType PluginType => PluginType.Monitoring;

    /// <inheritdoc />
    public override IReadOnlyList<string> RequiredPermissions { get; } = ["Observability.Read"];
}
