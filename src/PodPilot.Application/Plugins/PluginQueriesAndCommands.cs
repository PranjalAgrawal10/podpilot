using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Contracts.Plugins;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Plugins;

/// <summary>Lists marketplace catalog + installations.</summary>
public sealed class ListPluginsQuery : IRequest<IReadOnlyList<PluginResponse>>
{
}

/// <summary>Handles <see cref="ListPluginsQuery"/>.</summary>
public sealed class ListPluginsQueryHandler : IRequestHandler<ListPluginsQuery, IReadOnlyList<PluginResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IPluginInstaller installer;

    /// <summary>Initializes a new instance of the <see cref="ListPluginsQueryHandler"/> class.</summary>
    public ListPluginsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IPluginInstaller installer)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.installer = installer;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PluginResponse>> Handle(ListPluginsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginRead, cancellationToken);
        await installer.SyncCatalogAsync(cancellationToken);

        var definitions = await db.PluginDefinitions.AsNoTracking().Where(d => d.IsListed).OrderBy(d => d.Name).ToListAsync(cancellationToken);
        var installations = await db.PluginInstallations.AsNoTracking()
            .Where(i => i.OrganizationId == orgId)
            .ToDictionaryAsync(i => i.PluginDefinitionId, cancellationToken);

        return definitions.Select(d =>
            PluginMapper.ToCatalogResponse(d, installations.GetValueOrDefault(d.Id))).ToList();
    }
}

/// <summary>Gets plugin installation details.</summary>
public sealed class GetPluginQuery : IRequest<PluginResponse>
{
    public Guid InstallationId { get; init; }
}

/// <summary>Handles <see cref="GetPluginQuery"/>.</summary>
public sealed class GetPluginQueryHandler : IRequestHandler<GetPluginQuery, PluginResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="GetPluginQueryHandler"/> class.</summary>
    public GetPluginQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<PluginResponse> Handle(GetPluginQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginRead, cancellationToken);
        var installation = await db.PluginInstallations.AsNoTracking()
            .Include(i => i.PluginDefinition)
            .FirstOrDefaultAsync(i => i.Id == request.InstallationId && i.OrganizationId == orgId, cancellationToken)
            ?? throw new NotFoundException("Plugin installation", request.InstallationId);
        return PluginMapper.ToCatalogResponse(installation.PluginDefinition, installation);
    }
}

/// <summary>Gets plugin settings (secrets redacted).</summary>
public sealed class GetPluginSettingsQuery : IRequest<IReadOnlyList<PluginSettingResponse>>
{
    public Guid InstallationId { get; init; }
}

/// <summary>Handles <see cref="GetPluginSettingsQuery"/>.</summary>
public sealed class GetPluginSettingsQueryHandler : IRequestHandler<GetPluginSettingsQuery, IReadOnlyList<PluginSettingResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="GetPluginSettingsQueryHandler"/> class.</summary>
    public GetPluginSettingsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PluginSettingResponse>> Handle(GetPluginSettingsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginRead, cancellationToken);
        _ = await db.PluginInstallations.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InstallationId && i.OrganizationId == orgId, cancellationToken)
            ?? throw new NotFoundException("Plugin installation", request.InstallationId);

        var settings = await db.PluginSettings.AsNoTracking()
            .Where(s => s.PluginInstallationId == request.InstallationId && s.OrganizationId == orgId)
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken);

        return settings.Select(s => new PluginSettingResponse
        {
            Key = s.Key,
            Value = s.IsSecret ? null : s.Value,
            IsSecret = s.IsSecret,
            HasValue = !string.IsNullOrEmpty(s.Value),
        }).ToList();
    }
}

/// <summary>Gets plugin dashboard.</summary>
public sealed class GetPluginDashboardQuery : IRequest<PluginDashboardResponse>
{
}

/// <summary>Handles <see cref="GetPluginDashboardQuery"/>.</summary>
public sealed class GetPluginDashboardQueryHandler : IRequestHandler<GetPluginDashboardQuery, PluginDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IPluginManager pluginManager;

    /// <summary>Initializes a new instance of the <see cref="GetPluginDashboardQueryHandler"/> class.</summary>
    public GetPluginDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IPluginManager pluginManager)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.pluginManager = pluginManager;
    }

    /// <inheritdoc />
    public async Task<PluginDashboardResponse> Handle(GetPluginDashboardQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginRead, cancellationToken);
        var dash = await pluginManager.GetDashboardAsync(orgId, cancellationToken);
        return new PluginDashboardResponse
        {
            InstalledPlugins = dash.InstalledPlugins,
            EnabledPlugins = dash.EnabledPlugins,
            ConnectedMcpServers = dash.ConnectedMcpServers,
            AvailableTools = dash.AvailableTools,
            UnhealthyPlugins = dash.UnhealthyPlugins,
            RecentExecutions = dash.RecentExecutions,
        };
    }
}

/// <summary>Installs a plugin.</summary>
public sealed class InstallPluginCommand : IRequest<PluginResponse>
{
    public string PackageId { get; init; } = string.Empty;
    public IReadOnlyList<string> GrantedPermissions { get; init; } = [];
}

/// <summary>Handles <see cref="InstallPluginCommand"/>.</summary>
public sealed class InstallPluginCommandHandler : IRequestHandler<InstallPluginCommand, PluginResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IPluginInstaller installer;
    private readonly IApplicationDbContext db;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="InstallPluginCommandHandler"/> class.</summary>
    public InstallPluginCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IPluginInstaller installer,
        IApplicationDbContext db,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.installer = installer;
        this.db = db;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<PluginResponse> Handle(InstallPluginCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginManage, cancellationToken);
        var installationId = await installer.InstallAsync(orgId, request.PackageId.Trim(), request.GrantedPermissions, cancellationToken);
        var installation = await db.PluginInstallations.AsNoTracking()
            .Include(i => i.PluginDefinition)
            .FirstAsync(i => i.Id == installationId, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Created,
            nameof(PluginInstallation),
            installationId.ToString(),
            $"Plugin '{installation.PluginDefinition.PackageId}' installed",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
        return PluginMapper.ToCatalogResponse(installation.PluginDefinition, installation);
    }
}

/// <summary>Uninstalls a plugin.</summary>
public sealed class UninstallPluginCommand : IRequest
{
    public Guid InstallationId { get; init; }
}

/// <summary>Handles <see cref="UninstallPluginCommand"/>.</summary>
public sealed class UninstallPluginCommandHandler : IRequestHandler<UninstallPluginCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IPluginInstaller installer;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="UninstallPluginCommandHandler"/> class.</summary>
    public UninstallPluginCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IPluginInstaller installer,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.installer = installer;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task Handle(UninstallPluginCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginManage, cancellationToken);
        await installer.UninstallAsync(orgId, request.InstallationId, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(PluginInstallation),
            request.InstallationId.ToString(),
            "Plugin uninstalled",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
    }
}

/// <summary>Enables a plugin.</summary>
public sealed class EnablePluginCommand : IRequest
{
    public Guid InstallationId { get; init; }
}

/// <summary>Handles <see cref="EnablePluginCommand"/>.</summary>
public sealed class EnablePluginCommandHandler : IRequestHandler<EnablePluginCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IPluginManager pluginManager;

    /// <summary>Initializes a new instance of the <see cref="EnablePluginCommandHandler"/> class.</summary>
    public EnablePluginCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IPluginManager pluginManager)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.pluginManager = pluginManager;
    }

    /// <inheritdoc />
    public async Task Handle(EnablePluginCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginManage, cancellationToken);
        await pluginManager.EnableAsync(orgId, request.InstallationId, cancellationToken);
    }
}

/// <summary>Disables a plugin.</summary>
public sealed class DisablePluginCommand : IRequest
{
    public Guid InstallationId { get; init; }
}

/// <summary>Handles <see cref="DisablePluginCommand"/>.</summary>
public sealed class DisablePluginCommandHandler : IRequestHandler<DisablePluginCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IPluginManager pluginManager;

    /// <summary>Initializes a new instance of the <see cref="DisablePluginCommandHandler"/> class.</summary>
    public DisablePluginCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IPluginManager pluginManager)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.pluginManager = pluginManager;
    }

    /// <inheritdoc />
    public async Task Handle(DisablePluginCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginManage, cancellationToken);
        await pluginManager.DisableAsync(orgId, request.InstallationId, cancellationToken);
    }
}

/// <summary>Updates plugin settings.</summary>
public sealed class UpdatePluginSettingsCommand : IRequest
{
    public Guid InstallationId { get; init; }
    public IReadOnlyDictionary<string, string> Settings { get; init; } = new Dictionary<string, string>();
    public IReadOnlySet<string> SecretKeys { get; init; } = new HashSet<string>();
}

/// <summary>Handles <see cref="UpdatePluginSettingsCommand"/>.</summary>
public sealed class UpdatePluginSettingsCommandHandler : IRequestHandler<UpdatePluginSettingsCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IPluginManager pluginManager;

    /// <summary>Initializes a new instance of the <see cref="UpdatePluginSettingsCommandHandler"/> class.</summary>
    public UpdatePluginSettingsCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IPluginManager pluginManager)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.pluginManager = pluginManager;
    }

    /// <inheritdoc />
    public async Task Handle(UpdatePluginSettingsCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginManage, cancellationToken);
        await pluginManager.UpdateSettingsAsync(
            orgId,
            request.InstallationId,
            request.Settings,
            request.SecretKeys,
            cancellationToken);
    }
}

/// <summary>Updates granted permissions for an installation.</summary>
public sealed class UpdatePluginCommand : IRequest<PluginResponse>
{
    public Guid InstallationId { get; init; }
    public IReadOnlyList<string> GrantedPermissions { get; init; } = [];
}

/// <summary>Handles <see cref="UpdatePluginCommand"/>.</summary>
public sealed class UpdatePluginCommandHandler : IRequestHandler<UpdatePluginCommand, PluginResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IDateTimeService dateTimeService;

    /// <summary>Initializes a new instance of the <see cref="UpdatePluginCommandHandler"/> class.</summary>
    public UpdatePluginCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<PluginResponse> Handle(UpdatePluginCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PluginManage, cancellationToken);
        var installation = await db.PluginInstallations
            .Include(i => i.PluginDefinition)
            .FirstOrDefaultAsync(i => i.Id == request.InstallationId && i.OrganizationId == orgId, cancellationToken)
            ?? throw new NotFoundException("Plugin installation", request.InstallationId);

        var required = JsonSerializer.Deserialize<List<string>>(installation.PluginDefinition.RequiredPermissionsJson) ?? [];
        foreach (var permission in required)
        {
            if (!request.GrantedPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.GrantedPermissions),
                        $"Missing required plugin permission '{permission}'."),
                ]);
            }
        }

        installation.GrantedPermissionsJson = JsonSerializer.Serialize(request.GrantedPermissions);
        installation.UpdatedAt = dateTimeService.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return PluginMapper.ToCatalogResponse(installation.PluginDefinition, installation);
    }
}
