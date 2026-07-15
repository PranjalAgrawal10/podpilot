using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Routing.Commands.SimulateRouting;
using PodPilot.Application.Routing.Commands.UpdateRoutingPolicySettings;
using PodPilot.Application.Routing.Queries.GetRoutingDashboard;
using PodPilot.Application.Routing.Queries.GetRoutingPolicySettings;
using PodPilot.Application.Routing.Queries.ListRankedModels;
using PodPilot.Application.Routing.Queries.ListRoutingHistory;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Intelligent model router and cost optimizer endpoints.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class RoutingController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingController"/> class.
    /// </summary>
    public RoutingController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>Gets the routing dashboard.</summary>
    [HttpGet("api/v1/routing")]
    [ProducesResponseType(typeof(ApiResponse<RoutingDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoutingDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<RoutingDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets organization routing policy settings.</summary>
    [HttpGet("api/v1/routing/policy")]
    [ProducesResponseType(typeof(ApiResponse<RoutingPolicySettingsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicy(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoutingPolicySettingsQuery(), cancellationToken);
        return Ok(ApiResponse<RoutingPolicySettingsResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates organization routing policy settings.</summary>
    [HttpPut("api/v1/routing/policy")]
    [ProducesResponseType(typeof(ApiResponse<RoutingPolicySettingsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePolicy(
        [FromBody] UpdateRoutingPolicySettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<RoutingStrategy>(request.Strategy, ignoreCase: true, out var strategy))
        {
            throw new ValidationException($"Routing strategy '{request.Strategy}' is invalid.");
        }

        if (!Enum.TryParse<AiFailoverStrategy>(request.FailoverStrategy, ignoreCase: true, out var failover))
        {
            throw new ValidationException($"Failover strategy '{request.FailoverStrategy}' is invalid.");
        }

        var result = await mediator.Send(
            new UpdateRoutingPolicySettingsCommand
            {
                Strategy = strategy,
                CostWeight = request.CostWeight,
                LatencyWeight = request.LatencyWeight,
                ReliabilityWeight = request.ReliabilityWeight,
                ContextWeight = request.ContextWeight,
                FeaturesWeight = request.FeaturesWeight,
                AvailabilityWeight = request.AvailabilityWeight,
                MaxRetries = request.MaxRetries,
                FailoverStrategy = failover,
                PrimaryProviderId = request.PrimaryProviderId,
                FallbackProviderIds = request.FallbackProviderIds ?? [],
                PreferredTaskTypes = request.PreferredTaskTypes ?? [],
                CustomRulesJson = request.CustomRulesJson,
            },
            cancellationToken);

        return Ok(ApiResponse<RoutingPolicySettingsResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists ranked models for routing.</summary>
    [HttpGet("api/v1/routing/models")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RankedModelResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListModels(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListRankedModelsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RankedModelResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists routing decision history.</summary>
    [HttpGet("api/v1/routing/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoutingHistoryItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListHistory([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListRoutingHistoryQuery { Take = take }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoutingHistoryItemResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Simulates routing for a prompt.</summary>
    [HttpPost("api/v1/routing/simulate")]
    [ProducesResponseType(typeof(ApiResponse<SimulateRoutingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Simulate(
        [FromBody] SimulateRoutingRequest request,
        CancellationToken cancellationToken)
    {
        RoutingStrategy? strategy = null;
        if (!string.IsNullOrWhiteSpace(request.Strategy))
        {
            if (!Enum.TryParse<RoutingStrategy>(request.Strategy, ignoreCase: true, out var parsed))
            {
                throw new ValidationException($"Routing strategy '{request.Strategy}' is invalid.");
            }

            strategy = parsed;
        }

        var result = await mediator.Send(
            new SimulateRoutingCommand
            {
                Prompt = request.Prompt,
                Strategy = strategy,
                ModelHint = request.ModelHint,
                Path = request.Path,
            },
            cancellationToken);

        return Ok(ApiResponse<SimulateRoutingResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items.TryGetValue("CorrelationId", out var value) ? value?.ToString() : HttpContext.TraceIdentifier;
}
