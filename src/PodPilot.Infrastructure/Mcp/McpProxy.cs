using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Mcp;

/// <summary>
/// Proxies tool execution to MCP servers with retry and audit logging.
/// </summary>
public sealed class McpProxy : IMcpProxy
{
    private readonly IApplicationDbContext dbContext;
    private readonly IMcpRegistry mcpRegistry;
    private readonly IMcpConnectionFactory connectionFactory;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;
    private readonly IPluginNotificationService notificationService;
    private readonly ILogger<McpProxy> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpProxy"/> class.
    /// </summary>
    public McpProxy(
        IApplicationDbContext dbContext,
        IMcpRegistry mcpRegistry,
        IMcpConnectionFactory connectionFactory,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService,
        IPluginNotificationService notificationService,
        ILogger<McpProxy> logger)
    {
        this.dbContext = dbContext;
        this.mcpRegistry = mcpRegistry;
        this.connectionFactory = connectionFactory;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<McpToolCallResult> ExecuteToolAsync(
        McpToolCallRequest request,
        CancellationToken cancellationToken = default)
    {
        var resolved = await mcpRegistry.ResolveToolAsync(
            request.OrganizationId,
            request.ToolName,
            request.ServerId == Guid.Empty ? null : request.ServerId,
            cancellationToken);

        if (resolved is null)
        {
            throw new NotFoundException("MCP tool", request.ToolName);
        }

        var (serverRef, _) = resolved.Value;
        var server = await dbContext.McpServers
            .FirstOrDefaultAsync(
                s => s.Id == serverRef.Id && s.OrganizationId == request.OrganizationId,
                cancellationToken)
            ?? throw new NotFoundException("MCP server", serverRef.Id);

        string? credential = null;
        if (!string.IsNullOrWhiteSpace(server.EncryptedCredential))
        {
            credential = encryptionService.Decrypt(server.EncryptedCredential);
        }

        McpToolCallResult? lastResult = null;
        var attempts = Math.Max(1, server.MaxRetries + 1);
        var sw = Stopwatch.StartNew();

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                await using var connection = await connectionFactory.CreateAsync(server, credential, cancellationToken);
                await connection.ConnectAsync(cancellationToken);
                lastResult = await connection.CallToolAsync(request.ToolName, request.ArgumentsJson, cancellationToken);
                if (lastResult.Succeeded)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MCP tool attempt {Attempt} failed for {Tool}", attempt + 1, request.ToolName);
                lastResult = new McpToolCallResult
                {
                    Succeeded = false,
                    ErrorMessage = "Tool execution failed",
                    DurationMs = (int)sw.ElapsedMilliseconds,
                };
            }
        }

        sw.Stop();
        var result = lastResult ?? new McpToolCallResult
        {
            Succeeded = false,
            ErrorMessage = "Tool execution failed",
            DurationMs = (int)sw.ElapsedMilliseconds,
        };

        await dbContext.AddMcpToolExecutionAsync(
            new McpToolExecution
            {
                OrganizationId = request.OrganizationId,
                McpServerId = server.Id,
                ToolName = request.ToolName,
                Succeeded = result.Succeeded,
                DurationMs = result.DurationMs,
                ErrorMessage = result.ErrorMessage,
                ExecutedAt = dateTimeService.UtcNow,
                CreatedAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        server.Status = result.Succeeded ? McpConnectionStatus.Connected : McpConnectionStatus.Degraded;
        server.LastCheckedAt = dateTimeService.UtcNow;
        if (!result.Succeeded)
        {
            server.LastError = result.ErrorMessage;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyToolExecutedAsync(
            request.OrganizationId,
            server.Id,
            request.ToolName,
            result.Succeeded,
            cancellationToken);

        logger.LogInformation(
            "MCP tool {Tool} on {ServerId} succeeded={Succeeded} duration={DurationMs}",
            request.ToolName,
            server.Id,
            result.Succeeded,
            result.DurationMs);

        return result;
    }
}
