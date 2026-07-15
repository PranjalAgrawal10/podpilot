using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Mcp;

/// <summary>
/// Discovers and resolves MCP capabilities for organizations.
/// </summary>
public sealed class McpRegistry : IMcpRegistry
{
    private readonly IApplicationDbContext dbContext;
    private readonly IMcpConnectionFactory connectionFactory;
    private readonly IEncryptionService encryptionService;
    private readonly IMcpServerKindCatalog kindCatalog;
    private readonly IDateTimeService dateTimeService;
    private readonly IPluginNotificationService notificationService;
    private readonly ILogger<McpRegistry> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpRegistry"/> class.
    /// </summary>
    public McpRegistry(
        IApplicationDbContext dbContext,
        IMcpConnectionFactory connectionFactory,
        IEncryptionService encryptionService,
        IMcpServerKindCatalog kindCatalog,
        IDateTimeService dateTimeService,
        IPluginNotificationService notificationService,
        ILogger<McpRegistry> logger)
    {
        this.dbContext = dbContext;
        this.connectionFactory = connectionFactory;
        this.encryptionService = encryptionService;
        this.kindCatalog = kindCatalog;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<McpServerKindMetadata> ListKinds() => kindCatalog.GetAll();

    /// <inheritdoc />
    public async Task DiscoverAsync(
        Guid organizationId,
        Guid serverId,
        CancellationToken cancellationToken = default)
    {
        var server = await dbContext.McpServers
            .FirstOrDefaultAsync(s => s.Id == serverId && s.OrganizationId == organizationId, cancellationToken)
            ?? throw new NotFoundException("MCP server", serverId);

        string? credential = null;
        if (!string.IsNullOrWhiteSpace(server.EncryptedCredential))
        {
            credential = encryptionService.Decrypt(server.EncryptedCredential);
        }

        await using var connection = await connectionFactory.CreateAsync(server, credential, cancellationToken);
        try
        {
            await connection.ConnectAsync(cancellationToken);
            var tools = await connection.ListToolsAsync(cancellationToken);
            var resources = await connection.ListResourcesAsync(cancellationToken);
            var prompts = await connection.ListPromptsAsync(cancellationToken);

            await dbContext.ClearMcpServerCapabilitiesAsync(server.Id, cancellationToken);

            var now = dateTimeService.UtcNow;
            foreach (var tool in tools)
            {
                await dbContext.AddMcpToolAsync(
                    new McpTool
                    {
                        OrganizationId = organizationId,
                        McpServerId = server.Id,
                        Name = tool.Name,
                        Description = tool.Description,
                        InputSchemaJson = tool.InputSchemaJson,
                        IsEnabled = true,
                        DiscoveredAt = now,
                        CreatedAt = now,
                    },
                    cancellationToken);
            }

            foreach (var resource in resources)
            {
                await dbContext.AddMcpResourceAsync(
                    new McpResource
                    {
                        OrganizationId = organizationId,
                        McpServerId = server.Id,
                        Uri = resource.Uri,
                        Name = resource.Name,
                        MimeType = resource.MimeType,
                        Description = resource.Description,
                        DiscoveredAt = now,
                        CreatedAt = now,
                    },
                    cancellationToken);
            }

            foreach (var prompt in prompts)
            {
                await dbContext.AddMcpPromptAsync(
                    new McpPrompt
                    {
                        OrganizationId = organizationId,
                        McpServerId = server.Id,
                        Name = prompt.Name,
                        Description = prompt.Description,
                        ArgumentsJson = prompt.ArgumentsJson,
                        DiscoveredAt = now,
                        CreatedAt = now,
                    },
                    cancellationToken);
            }

            server.Status = McpConnectionStatus.Connected;
            server.LastCheckedAt = now;
            server.LastError = null;
            await dbContext.SaveChangesAsync(cancellationToken);
            await notificationService.NotifyMcpConnectedAsync(organizationId, server.Id, cancellationToken);
            logger.LogInformation(
                "MCP discovery completed for server {ServerId}: {ToolCount} tools",
                server.Id,
                tools.Count);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            server.Status = McpConnectionStatus.Disconnected;
            server.LastCheckedAt = dateTimeService.UtcNow;
            server.LastError = "Discovery failed";
            await dbContext.SaveChangesAsync(cancellationToken);
            await notificationService.NotifyMcpDisconnectedAsync(organizationId, server.Id, cancellationToken);
            logger.LogWarning(ex, "MCP discovery failed for {ServerId}", server.Id);
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(serverId), "Unable to connect to MCP server."),
            ]);
        }
    }

    /// <inheritdoc />
    public async Task<(McpServer Server, McpTool Tool)?> ResolveToolAsync(
        Guid organizationId,
        string toolName,
        Guid? preferredServerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.McpTools
            .AsNoTracking()
            .Include(t => t.McpServer)
            .Where(t =>
                t.OrganizationId == organizationId &&
                t.IsEnabled &&
                t.McpServer.IsEnabled &&
                t.Name == toolName);

        if (preferredServerId.HasValue)
        {
            query = query.Where(t => t.McpServerId == preferredServerId.Value);
        }

        var tool = await query
            .OrderByDescending(t => t.McpServer.Status == McpConnectionStatus.Connected)
            .ThenBy(t => t.McpServer.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return tool is null ? null : (tool.McpServer, tool);
    }
}
