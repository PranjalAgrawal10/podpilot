using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Plugins;

/// <summary>
/// Manages plugin enable/disable, settings, and health.
/// </summary>
public sealed class PluginManager : IPluginManager
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPluginRegistry pluginRegistry;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;
    private readonly IPluginNotificationService notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    public PluginManager(
        IApplicationDbContext dbContext,
        IPluginRegistry pluginRegistry,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService,
        IPluginNotificationService notificationService)
    {
        this.dbContext = dbContext;
        this.pluginRegistry = pluginRegistry;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task EnableAsync(
        Guid organizationId,
        Guid installationId,
        CancellationToken cancellationToken = default)
    {
        var installation = await GetInstallationAsync(organizationId, installationId, cancellationToken);
        var context = await BuildContextAsync(installation, cancellationToken);
        var runtime = pluginRegistry.Get(installation.PluginDefinition.PackageId);
        if (runtime is not null)
        {
            await runtime.StartAsync(context, cancellationToken);
        }

        installation.Status = PluginStatus.Enabled;
        installation.EnabledAt = dateTimeService.UtcNow;
        installation.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.AddPluginLogAsync(
            new PluginLog
            {
                OrganizationId = organizationId,
                PluginInstallationId = installation.Id,
                Level = "Information",
                Category = "Lifecycle",
                Message = "Plugin enabled",
                OccurredAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyPluginUpdatedAsync(organizationId, installationId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DisableAsync(
        Guid organizationId,
        Guid installationId,
        CancellationToken cancellationToken = default)
    {
        var installation = await GetInstallationAsync(organizationId, installationId, cancellationToken);
        var context = await BuildContextAsync(installation, cancellationToken);
        var runtime = pluginRegistry.Get(installation.PluginDefinition.PackageId);
        if (runtime is not null)
        {
            await runtime.StopAsync(context, cancellationToken);
        }

        installation.Status = PluginStatus.Disabled;
        installation.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.AddPluginLogAsync(
            new PluginLog
            {
                OrganizationId = organizationId,
                PluginInstallationId = installation.Id,
                Level = "Information",
                Category = "Lifecycle",
                Message = "Plugin disabled",
                OccurredAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyPluginUpdatedAsync(organizationId, installationId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateSettingsAsync(
        Guid organizationId,
        Guid installationId,
        IReadOnlyDictionary<string, string> settings,
        IReadOnlySet<string> secretKeys,
        CancellationToken cancellationToken = default)
    {
        var installation = await GetInstallationAsync(organizationId, installationId, cancellationToken);
        var existing = await dbContext.PluginSettings
            .Where(s => s.PluginInstallationId == installation.Id)
            .ToListAsync(cancellationToken);

        foreach (var (key, value) in settings)
        {
            var isSecret = secretKeys.Contains(key);
            var storedValue = isSecret ? encryptionService.Encrypt(value) : value;
            var row = existing.FirstOrDefault(s => s.Key == key);
            if (row is null)
            {
                await dbContext.AddPluginSettingAsync(
                    new PluginSetting
                    {
                        OrganizationId = organizationId,
                        PluginInstallationId = installation.Id,
                        Key = key,
                        Value = storedValue,
                        IsSecret = isSecret,
                        CreatedAt = dateTimeService.UtcNow,
                    },
                    cancellationToken);
            }
            else
            {
                row.Value = storedValue;
                row.IsSecret = isSecret;
                row.UpdatedAt = dateTimeService.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyPluginUpdatedAsync(organizationId, installationId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CheckHealthAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var installations = await dbContext.PluginInstallations
            .Include(i => i.PluginDefinition)
            .Where(i => i.OrganizationId == organizationId && i.Status == PluginStatus.Enabled)
            .ToListAsync(cancellationToken);

        foreach (var installation in installations)
        {
            var runtime = pluginRegistry.Get(installation.PluginDefinition.PackageId);
            if (runtime is null)
            {
                installation.IsHealthy = false;
                installation.HealthMessage = "Runtime not loaded";
                continue;
            }

            var context = await BuildContextAsync(installation, cancellationToken);
            var health = await runtime.CheckHealthAsync(context, cancellationToken);
            installation.IsHealthy = health.IsHealthy;
            installation.HealthMessage = health.Message;
            installation.LastHealthCheckAt = dateTimeService.UtcNow;
            if (!health.IsHealthy)
            {
                installation.Status = PluginStatus.Failed;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PluginDashboard> GetDashboardAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var since = dateTimeService.UtcNow.AddDays(-7);
        var installed = await dbContext.PluginInstallations.CountAsync(
            i => i.OrganizationId == organizationId, cancellationToken);
        var enabled = await dbContext.PluginInstallations.CountAsync(
            i => i.OrganizationId == organizationId && i.Status == PluginStatus.Enabled, cancellationToken);
        var unhealthy = await dbContext.PluginInstallations.CountAsync(
            i => i.OrganizationId == organizationId && !i.IsHealthy, cancellationToken);
        var connected = await dbContext.McpServers.CountAsync(
            s => s.OrganizationId == organizationId &&
                 s.IsEnabled &&
                 s.Status == McpConnectionStatus.Connected,
            cancellationToken);
        var tools = await dbContext.McpTools.CountAsync(
            t => t.OrganizationId == organizationId && t.IsEnabled, cancellationToken);
        var executions = await dbContext.McpToolExecutions.CountAsync(
            e => e.OrganizationId == organizationId && e.ExecutedAt >= since, cancellationToken);

        return new PluginDashboard
        {
            InstalledPlugins = installed,
            EnabledPlugins = enabled,
            ConnectedMcpServers = connected,
            AvailableTools = tools,
            UnhealthyPlugins = unhealthy,
            RecentExecutions = executions,
        };
    }

    private async Task<PluginInstallation> GetInstallationAsync(
        Guid organizationId,
        Guid installationId,
        CancellationToken cancellationToken)
    {
        var installation = await dbContext.PluginInstallations
            .Include(i => i.PluginDefinition)
            .FirstOrDefaultAsync(
                i => i.Id == installationId && i.OrganizationId == organizationId,
                cancellationToken);
        return installation ?? throw new NotFoundException("Plugin installation", installationId);
    }

    private async Task<PluginContext> BuildContextAsync(
        PluginInstallation installation,
        CancellationToken cancellationToken)
    {
        var settings = await dbContext.PluginSettings
            .AsNoTracking()
            .Where(s => s.PluginInstallationId == installation.Id && !s.IsSecret)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var granted = JsonSerializer.Deserialize<List<string>>(installation.GrantedPermissionsJson) ?? [];
        return new PluginContext
        {
            OrganizationId = installation.OrganizationId,
            InstallationId = installation.Id,
            GrantedPermissions = granted,
            Settings = settings,
        };
    }
}
