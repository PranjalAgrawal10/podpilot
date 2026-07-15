using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Mcp;
using PodPilot.Infrastructure.Plugins;
using PodPilot.Infrastructure.Plugins.BuiltIn;

namespace PodPilot.Infrastructure;

/// <summary>
/// Plugin system and MCP dependency injection.
/// </summary>
public static class PluginsDependencyInjection
{
    /// <summary>
    /// Registers plugin and MCP services.
    /// </summary>
    public static IServiceCollection AddPluginSystem(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddHttpClient(nameof(HttpJsonRpcMcpConnection));

        services.AddSingleton<IPluginRegistry, PluginRegistry>();
        services.AddSingleton<IMcpServerKindCatalog, McpServerKindCatalog>();
        services.AddSingleton<IPlugin, UtilityHealthPlugin>();
        services.AddSingleton<IPlugin, NotificationAuditPlugin>();
        services.AddSingleton<IPlugin, McpBridgePlugin>();
        services.AddSingleton<IPlugin, MonitoringTelemetryPlugin>();

        services.AddScoped<IPluginLoader, PluginLoader>();
        services.AddScoped<IPluginInstaller, PluginInstaller>();
        services.AddScoped<IPluginManager, PluginManager>();
        services.AddScoped<IMcpConnectionFactory, McpConnectionFactory>();
        services.AddScoped<IMcpRegistry, McpRegistry>();
        services.AddScoped<IMcpProxy, McpProxy>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IPluginNotificationService, NoOpPluginNotificationService>();
        }
        else
        {
            services.AddScoped<IPluginNotificationService, PluginNotificationService>();
        }

        return services;
    }
}
