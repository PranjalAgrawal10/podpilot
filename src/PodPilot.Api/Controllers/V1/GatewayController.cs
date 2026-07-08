using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Gateway.Commands.CreateGatewayApiKey;
using PodPilot.Application.Gateway.Commands.CreateGatewayRoute;
using PodPilot.Application.Gateway.Commands.DeleteGatewayRoute;
using PodPilot.Application.Gateway.Commands.RevokeGatewayApiKey;
using PodPilot.Application.Gateway.Commands.RotateGatewayApiKey;
using PodPilot.Application.Gateway.Queries.GetGatewayStats;
using PodPilot.Application.Gateway.Queries.ListGatewayApiKeys;
using PodPilot.Application.Gateway.Queries.ListGatewayRequests;
using PodPilot.Application.Gateway.Queries.ListGatewayRoutes;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Gateway management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/gateway")]
[Authorize]
[Produces("application/json")]
public sealed class GatewayController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayController"/> class.
    /// </summary>
    public GatewayController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists gateway API keys.
    /// </summary>
    [HttpGet("api-keys")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GatewayApiKeyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListApiKeys(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListGatewayApiKeysQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GatewayApiKeyResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a gateway API key.
    /// </summary>
    [HttpPost("api-keys")]
    [ProducesResponseType(typeof(ApiResponse<GatewayApiKeyResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApiKey(
        [FromBody] CreateGatewayApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateGatewayApiKeyCommand
            {
                Name = request.Name,
                IsPersonal = request.IsPersonal,
                ExpiresAt = request.ExpiresAt,
                RateLimitPerMinute = request.RateLimitPerMinute,
                RateLimitPerDay = request.RateLimitPerDay,
            },
            cancellationToken);

        return Created(
            $"/api/v1/gateway/api-keys/{result.Id}",
            ApiResponse<GatewayApiKeyResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Revokes a gateway API key.
    /// </summary>
    [HttpDelete("api-keys/{keyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeApiKey(Guid keyId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeGatewayApiKeyCommand { KeyId = keyId }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Rotates a gateway API key.
    /// </summary>
    [HttpPost("api-keys/{keyId:guid}/rotate")]
    [ProducesResponseType(typeof(ApiResponse<GatewayApiKeyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RotateApiKey(Guid keyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RotateGatewayApiKeyCommand { KeyId = keyId }, cancellationToken);
        return Ok(ApiResponse<GatewayApiKeyResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists gateway routes.
    /// </summary>
    [HttpGet("routes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GatewayRouteResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRoutes(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListGatewayRoutesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GatewayRouteResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a gateway route.
    /// </summary>
    [HttpPost("routes")]
    [ProducesResponseType(typeof(ApiResponse<GatewayRouteResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoute(
        [FromBody] CreateGatewayRouteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateGatewayRouteCommand
            {
                GpuPodId = request.GpuPodId,
                ModelName = request.ModelName,
                IsDefault = request.IsDefault,
            },
            cancellationToken);

        return Created(
            $"/api/v1/gateway/routes/{result.Id}",
            ApiResponse<GatewayRouteResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Deletes a gateway route.
    /// </summary>
    [HttpDelete("routes/{routeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRoute(Guid routeId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteGatewayRouteCommand { RouteId = routeId }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets gateway dashboard statistics.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<GatewayStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetGatewayStatsQuery(), cancellationToken);
        return Ok(ApiResponse<GatewayStatsResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists recent gateway requests.
    /// </summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GatewayRequestSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRequests(
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListGatewayRequestsQuery { Limit = limit }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GatewayRequestSummaryResponse>>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
