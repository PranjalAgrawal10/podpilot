using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Contracts.Plugins;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Plugins;

/// <summary>Lists MCP servers.</summary>
public sealed class ListMcpServersQuery : IRequest<IReadOnlyList<McpServerResponse>>
{
}

/// <summary>Handles <see cref="ListMcpServersQuery"/>.</summary>
public sealed class ListMcpServersQueryHandler : IRequestHandler<ListMcpServersQuery, IReadOnlyList<McpServerResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListMcpServersQueryHandler"/> class.</summary>
    public ListMcpServersQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpServerResponse>> Handle(ListMcpServersQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpRead, cancellationToken);
        var servers = await db.McpServers.AsNoTracking()
            .Include(s => s.Tools)
            .Include(s => s.Resources)
            .Where(s => s.OrganizationId == orgId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
        return servers.Select(PluginMapper.ToMcpServerResponse).ToList();
    }
}

/// <summary>Lists MCP tools.</summary>
public sealed class ListMcpToolsQuery : IRequest<IReadOnlyList<McpToolResponse>>
{
}

/// <summary>Handles <see cref="ListMcpToolsQuery"/>.</summary>
public sealed class ListMcpToolsQueryHandler : IRequestHandler<ListMcpToolsQuery, IReadOnlyList<McpToolResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListMcpToolsQueryHandler"/> class.</summary>
    public ListMcpToolsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpToolResponse>> Handle(ListMcpToolsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpRead, cancellationToken);
        var tools = await db.McpTools.AsNoTracking()
            .Include(t => t.McpServer)
            .Where(t => t.OrganizationId == orgId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
        return tools.Select(PluginMapper.ToMcpToolResponse).ToList();
    }
}

/// <summary>Lists MCP resources.</summary>
public sealed class ListMcpResourcesQuery : IRequest<IReadOnlyList<McpResourceResponse>>
{
}

/// <summary>Handles <see cref="ListMcpResourcesQuery"/>.</summary>
public sealed class ListMcpResourcesQueryHandler : IRequestHandler<ListMcpResourcesQuery, IReadOnlyList<McpResourceResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListMcpResourcesQueryHandler"/> class.</summary>
    public ListMcpResourcesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpResourceResponse>> Handle(ListMcpResourcesQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpRead, cancellationToken);
        var resources = await db.McpResources.AsNoTracking()
            .Include(r => r.McpServer)
            .Where(r => r.OrganizationId == orgId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
        return resources.Select(PluginMapper.ToMcpResourceResponse).ToList();
    }
}

/// <summary>Lists MCP server kinds.</summary>
public sealed class ListMcpKindsQuery : IRequest<IReadOnlyList<McpServerKindResponse>>
{
}

/// <summary>Handles <see cref="ListMcpKindsQuery"/>.</summary>
public sealed class ListMcpKindsQueryHandler : IRequestHandler<ListMcpKindsQuery, IReadOnlyList<McpServerKindResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IMcpRegistry mcpRegistry;

    /// <summary>Initializes a new instance of the <see cref="ListMcpKindsQueryHandler"/> class.</summary>
    public ListMcpKindsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IMcpRegistry mcpRegistry)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.mcpRegistry = mcpRegistry;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpServerKindResponse>> Handle(ListMcpKindsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpRead, cancellationToken);
        await Task.CompletedTask;
        return mcpRegistry.ListKinds().Select(k => new McpServerKindResponse
        {
            ServerKind = k.ServerKind.ToString(),
            DisplayName = k.DisplayName,
            Description = k.Description,
            DefaultEndpoint = k.DefaultEndpoint,
            DefaultAuthScheme = k.DefaultAuthScheme,
            RequiresCredential = k.RequiresCredential,
        }).ToList();
    }
}

/// <summary>Creates an MCP server.</summary>
public sealed class CreateMcpServerCommand : IRequest<McpServerResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public McpServerKind ServerKind { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string AuthScheme { get; init; } = "None";
    public string? Credential { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 2;
    public bool SupportsStreaming { get; init; } = true;
    public bool DiscoverOnCreate { get; init; } = true;
}

/// <summary>Handles <see cref="CreateMcpServerCommand"/>.</summary>
public sealed class CreateMcpServerCommandHandler : IRequestHandler<CreateMcpServerCommand, McpServerResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;
    private readonly IMcpRegistry mcpRegistry;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="CreateMcpServerCommandHandler"/> class.</summary>
    public CreateMcpServerCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService,
        IMcpRegistry mcpRegistry,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
        this.mcpRegistry = mcpRegistry;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<McpServerResponse> Handle(CreateMcpServerCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpManage, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > ApplicationConstants.McpServerNameMaxLength)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), "A valid MCP server name is required."),
            ]);
        }

        if (string.IsNullOrWhiteSpace(request.Endpoint) || request.Endpoint.Length > ApplicationConstants.McpEndpointMaxLength)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Endpoint), "A valid MCP endpoint is required."),
            ]);
        }

        var exists = await db.McpServers.AnyAsync(
            s => s.OrganizationId == orgId && s.Name == request.Name.Trim(),
            cancellationToken);
        if (exists)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), "An MCP server with this name already exists."),
            ]);
        }

        var now = dateTimeService.UtcNow;
        var server = new McpServer
        {
            OrganizationId = orgId,
            Name = request.Name.Trim(),
            Version = string.IsNullOrWhiteSpace(request.Version) ? "1.0.0" : request.Version.Trim(),
            ServerKind = request.ServerKind,
            Endpoint = request.Endpoint.Trim(),
            AuthScheme = string.IsNullOrWhiteSpace(request.AuthScheme) ? "None" : request.AuthScheme.Trim(),
            EncryptedCredential = string.IsNullOrWhiteSpace(request.Credential)
                ? null
                : encryptionService.Encrypt(request.Credential),
            TimeoutSeconds = Math.Clamp(request.TimeoutSeconds, 5, 300),
            MaxRetries = Math.Clamp(request.MaxRetries, 0, 10),
            SupportsStreaming = request.SupportsStreaming,
            IsEnabled = true,
            Status = McpConnectionStatus.Unknown,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        await db.AddMcpServerAsync(server, cancellationToken);
        await db.AddPluginLogAsync(
            new PluginLog
            {
                OrganizationId = orgId,
                Level = "Information",
                Category = "McpRegistration",
                Message = $"Registered MCP server '{server.Name}'",
                OccurredAt = now,
                CreatedAt = now,
            },
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        if (request.DiscoverOnCreate)
        {
            try
            {
                await mcpRegistry.DiscoverAsync(orgId, server.Id, cancellationToken);
            }
            catch (ValidationException)
            {
                // Server remains registered; discovery can be retried.
            }
        }

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(McpServer),
            server.Id.ToString(),
            $"MCP server '{server.Name}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        server = await db.McpServers.AsNoTracking()
            .Include(s => s.Tools)
            .Include(s => s.Resources)
            .FirstAsync(s => s.Id == server.Id, cancellationToken);
        return PluginMapper.ToMcpServerResponse(server);
    }
}

/// <summary>Deletes an MCP server.</summary>
public sealed class DeleteMcpServerCommand : IRequest
{
    public Guid ServerId { get; init; }
}

/// <summary>Handles <see cref="DeleteMcpServerCommand"/>.</summary>
public sealed class DeleteMcpServerCommandHandler : IRequestHandler<DeleteMcpServerCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IPluginNotificationService notificationService;

    /// <summary>Initializes a new instance of the <see cref="DeleteMcpServerCommandHandler"/> class.</summary>
    public DeleteMcpServerCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IPluginNotificationService notificationService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteMcpServerCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpManage, cancellationToken);
        var server = await db.McpServers.FirstOrDefaultAsync(
            s => s.Id == request.ServerId && s.OrganizationId == orgId,
            cancellationToken)
            ?? throw new NotFoundException("MCP server", request.ServerId);

        await db.RemoveMcpServerAsync(server.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyMcpDisconnectedAsync(orgId, request.ServerId, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(McpServer),
            request.ServerId.ToString(),
            $"MCP server '{server.Name}' deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
    }
}

/// <summary>Executes an MCP tool.</summary>
public sealed class ExecuteMcpToolCommand : IRequest<ExecuteMcpToolResponse>
{
    public Guid? ServerId { get; init; }
    public string ToolName { get; init; } = string.Empty;
    public string ArgumentsJson { get; init; } = "{}";
}

/// <summary>Handles <see cref="ExecuteMcpToolCommand"/>.</summary>
public sealed class ExecuteMcpToolCommandHandler : IRequestHandler<ExecuteMcpToolCommand, ExecuteMcpToolResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IMcpProxy mcpProxy;

    /// <summary>Initializes a new instance of the <see cref="ExecuteMcpToolCommandHandler"/> class.</summary>
    public ExecuteMcpToolCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IMcpProxy mcpProxy)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.mcpProxy = mcpProxy;
    }

    /// <inheritdoc />
    public async Task<ExecuteMcpToolResponse> Handle(ExecuteMcpToolCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = PluginAccess.RequireOrganizationContext(currentUserService);
        await PluginAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.McpManage, cancellationToken);
        var result = await mcpProxy.ExecuteToolAsync(
            new McpToolCallRequest
            {
                OrganizationId = orgId,
                ServerId = request.ServerId ?? Guid.Empty,
                ToolName = request.ToolName.Trim(),
                ArgumentsJson = string.IsNullOrWhiteSpace(request.ArgumentsJson) ? "{}" : request.ArgumentsJson,
            },
            cancellationToken);

        return new ExecuteMcpToolResponse
        {
            Succeeded = result.Succeeded,
            ContentJson = result.ContentJson,
            ErrorMessage = result.ErrorMessage,
            DurationMs = result.DurationMs,
        };
    }
}
