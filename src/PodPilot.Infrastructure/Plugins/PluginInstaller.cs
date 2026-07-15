using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Plugins;

/// <summary>
/// Installs plugin packages into organizations with permission sandboxing.
/// </summary>
public sealed class PluginInstaller : IPluginInstaller
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPluginLoader pluginLoader;
    private readonly IPluginRegistry pluginRegistry;
    private readonly IDateTimeService dateTimeService;
    private readonly IPluginNotificationService notificationService;
    private readonly ILogger<PluginInstaller> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstaller"/> class.
    /// </summary>
    public PluginInstaller(
        IApplicationDbContext dbContext,
        IPluginLoader pluginLoader,
        IPluginRegistry pluginRegistry,
        IDateTimeService dateTimeService,
        IPluginNotificationService notificationService,
        ILogger<PluginInstaller> logger)
    {
        this.dbContext = dbContext;
        this.pluginLoader = pluginLoader;
        this.pluginRegistry = pluginRegistry;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task SyncCatalogAsync(CancellationToken cancellationToken = default)
    {
        var packages = await pluginLoader.LoadAsync(cancellationToken);
        foreach (var plugin in packages)
        {
            pluginRegistry.Register(plugin);
            var existing = await dbContext.PluginDefinitions
                .FirstOrDefaultAsync(d => d.PackageId == plugin.PackageId, cancellationToken);

            if (existing is null)
            {
                await dbContext.AddPluginDefinitionAsync(
                    new PluginDefinition
                    {
                        PackageId = plugin.PackageId,
                        Name = plugin.Name,
                        Version = plugin.Version,
                        PluginType = plugin.PluginType,
                        Description = $"First-party {plugin.PluginType} plugin",
                        Publisher = "PodPilot",
                        IsFirstParty = true,
                        RequiredPermissionsJson = JsonSerializer.Serialize(plugin.RequiredPermissions),
                        IsListed = true,
                        CreatedAt = dateTimeService.UtcNow,
                    },
                    cancellationToken);
            }
            else
            {
                existing.Name = plugin.Name;
                existing.Version = plugin.Version;
                existing.PluginType = plugin.PluginType;
                existing.RequiredPermissionsJson = JsonSerializer.Serialize(plugin.RequiredPermissions);
                existing.UpdatedAt = dateTimeService.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> InstallAsync(
        Guid organizationId,
        string packageId,
        IReadOnlyList<string>? grantedPermissions = null,
        CancellationToken cancellationToken = default)
    {
        await SyncCatalogAsync(cancellationToken);

        var definition = await dbContext.PluginDefinitions
            .FirstOrDefaultAsync(d => d.PackageId == packageId, cancellationToken)
            ?? throw new NotFoundException("Plugin", packageId);

        var already = await dbContext.PluginInstallations.AnyAsync(
            i => i.OrganizationId == organizationId && i.PluginDefinitionId == definition.Id,
            cancellationToken);
        if (already)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(packageId), "Plugin is already installed."),
            ]);
        }

        var required = JsonSerializer.Deserialize<List<string>>(definition.RequiredPermissionsJson) ?? [];
        var granted = grantedPermissions?.ToList() ?? required;
        foreach (var permission in required)
        {
            if (!granted.Contains(permission, StringComparer.OrdinalIgnoreCase))
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(grantedPermissions),
                        $"Missing required plugin permission '{permission}'."),
                ]);
            }
        }

        var now = dateTimeService.UtcNow;
        var installation = new PluginInstallation
        {
            OrganizationId = organizationId,
            PluginDefinitionId = definition.Id,
            Status = PluginStatus.Installed,
            GrantedPermissionsJson = JsonSerializer.Serialize(granted),
            IsHealthy = true,
            CreatedAt = now,
        };

        await dbContext.AddPluginInstallationAsync(installation, cancellationToken);
        await dbContext.AddPluginLogAsync(
            new PluginLog
            {
                OrganizationId = organizationId,
                PluginInstallationId = installation.Id,
                Level = "Information",
                Category = "Installation",
                Message = $"Installed plugin {definition.PackageId}",
                OccurredAt = now,
                CreatedAt = now,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var runtime = pluginRegistry.Get(definition.PackageId);
        if (runtime is not null)
        {
            await runtime.InitializeAsync(
                new PluginContext
                {
                    OrganizationId = organizationId,
                    InstallationId = installation.Id,
                    GrantedPermissions = granted,
                },
                cancellationToken);
        }

        logger.LogInformation(
            "Plugin {PackageId} installed for org {OrganizationId}",
            packageId,
            organizationId);
        await notificationService.NotifyPluginInstalledAsync(organizationId, installation.Id, cancellationToken);
        return installation.Id;
    }

    /// <inheritdoc />
    public async Task UninstallAsync(
        Guid organizationId,
        Guid installationId,
        CancellationToken cancellationToken = default)
    {
        var installation = await dbContext.PluginInstallations
            .Include(i => i.PluginDefinition)
            .FirstOrDefaultAsync(
                i => i.Id == installationId && i.OrganizationId == organizationId,
                cancellationToken)
            ?? throw new NotFoundException("Plugin installation", installationId);

        var runtime = pluginRegistry.Get(installation.PluginDefinition.PackageId);
        if (runtime is not null && installation.Status == PluginStatus.Enabled)
        {
            await runtime.StopAsync(
                new PluginContext
                {
                    OrganizationId = organizationId,
                    InstallationId = installation.Id,
                },
                cancellationToken);
        }

        await dbContext.AddPluginLogAsync(
            new PluginLog
            {
                OrganizationId = organizationId,
                Level = "Information",
                Category = "Removal",
                Message = $"Removed plugin {installation.PluginDefinition.PackageId}",
                OccurredAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        await dbContext.RemovePluginInstallationAsync(installationId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyPluginRemovedAsync(organizationId, installationId, cancellationToken);
    }
}
