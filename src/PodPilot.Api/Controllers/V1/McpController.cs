using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Plugins;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Plugins;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// MCP server and tool endpoints.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class McpController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpController"/> class.
    /// </summary>
    public McpController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Lists MCP servers.</summary>
    [HttpGet("api/v1/mcp/servers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<McpServerResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListServers(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListMcpServersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<McpServerResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists built-in MCP kinds.</summary>
    [HttpGet("api/v1/mcp/kinds")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<McpServerKindResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListKinds(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListMcpKindsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<McpServerKindResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Registers an MCP server.</summary>
    [HttpPost("api/v1/mcp/servers")]
    [ProducesResponseType(typeof(ApiResponse<McpServerResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateServer(
        [FromBody] CreateMcpServerRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<McpServerKind>(request.ServerKind, ignoreCase: true, out var kind))
        {
            throw new ValidationException($"MCP server kind '{request.ServerKind}' is invalid.");
        }

        var result = await mediator.Send(
            new CreateMcpServerCommand
            {
                Name = request.Name,
                Version = request.Version,
                ServerKind = kind,
                Endpoint = request.Endpoint,
                AuthScheme = request.AuthScheme,
                Credential = request.Credential,
                TimeoutSeconds = request.TimeoutSeconds,
                MaxRetries = request.MaxRetries,
                SupportsStreaming = request.SupportsStreaming,
                DiscoverOnCreate = request.DiscoverOnCreate,
            },
            cancellationToken);

        return CreatedAtAction(nameof(ListServers), ApiResponse<McpServerResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Deletes an MCP server.</summary>
    [HttpDelete("api/v1/mcp/servers/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteServer(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteMcpServerCommand { ServerId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Lists MCP tools.</summary>
    [HttpGet("api/v1/mcp/tools")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<McpToolResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTools(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListMcpToolsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<McpToolResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists MCP resources.</summary>
    [HttpGet("api/v1/mcp/resources")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<McpResourceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListResources(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListMcpResourcesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<McpResourceResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Executes an MCP tool.</summary>
    [HttpPost("api/v1/mcp/tools/execute")]
    [ProducesResponseType(typeof(ApiResponse<ExecuteMcpToolResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteTool(
        [FromBody] ExecuteMcpToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ExecuteMcpToolCommand
            {
                ServerId = request.ServerId,
                ToolName = request.ToolName,
                ArgumentsJson = request.ArgumentsJson,
            },
            cancellationToken);
        return Ok(ApiResponse<ExecuteMcpToolResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items.TryGetValue("CorrelationId", out var value) ? value?.ToString() : HttpContext.TraceIdentifier;
}
