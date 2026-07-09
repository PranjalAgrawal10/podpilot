using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Application.Orchestration.Commands.ScaleDown;
using PodPilot.Application.Orchestration.Commands.ScaleUp;
using PodPilot.Application.Orchestration.Commands.UpdateLoadBalancerConfig;
using PodPilot.Application.Orchestration.Queries.GetAutoScalerStatus;
using PodPilot.Application.Orchestration.Queries.GetCapacity;
using PodPilot.Application.Orchestration.Queries.GetLoadBalancerConfig;
using PodPilot.Application.Orchestration.Queries.GetOrchestratorStatus;
using PodPilot.Application.Orchestration.Queries.ListPodHealthMetrics;
using PodPilot.Application.Orchestration.Queries.ListScalingEvents;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Multi-pod orchestration endpoints.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class OrchestratorController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrchestratorController"/> class.
    /// </summary>
    public OrchestratorController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Gets orchestrator status for the current organization.
    /// </summary>
    [HttpGet("api/v1/orchestrator")]
    [ProducesResponseType(typeof(ApiResponse<OrchestratorStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrchestratorStatus(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrchestratorStatusQuery(), cancellationToken);
        return Ok(ApiResponse<OrchestratorStatusResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets auto-scaler status for the current organization.
    /// </summary>
    [HttpGet("api/v1/autoscaler")]
    [ProducesResponseType(typeof(ApiResponse<AutoScalerStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAutoScalerStatus(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAutoScalerStatusQuery(), cancellationToken);
        return Ok(ApiResponse<AutoScalerStatusResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets capacity planning data.
    /// </summary>
    [HttpGet("api/v1/capacity")]
    [ProducesResponseType(typeof(ApiResponse<CapacityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCapacity([FromQuery] Guid? poolId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCapacityQuery { PoolId = poolId }, cancellationToken);
        return Ok(ApiResponse<CapacityResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Manually scales up a pod pool.
    /// </summary>
    [HttpPost("api/v1/autoscaler/scale-up")]
    [ProducesResponseType(typeof(ApiResponse<ScalingActionResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ScaleUp(
        [FromBody] ManualScaleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ScaleUpCommand { PoolId = request.PoolId, Reason = request.Reason },
            cancellationToken);
        return Ok(ApiResponse<ScalingActionResult>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Manually scales down a pod pool.
    /// </summary>
    [HttpPost("api/v1/autoscaler/scale-down")]
    [ProducesResponseType(typeof(ApiResponse<ScalingActionResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ScaleDown(
        [FromBody] ManualScaleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ScaleDownCommand { PoolId = request.PoolId, Reason = request.Reason },
            cancellationToken);
        return Ok(ApiResponse<ScalingActionResult>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets load balancer configuration.
    /// </summary>
    [HttpGet("api/v1/load-balancer")]
    [ProducesResponseType(typeof(ApiResponse<LoadBalancerConfigResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoadBalancerConfig(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLoadBalancerConfigQuery(), cancellationToken);
        return Ok(ApiResponse<LoadBalancerConfigResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Updates load balancer configuration.
    /// </summary>
    [HttpPut("api/v1/load-balancer")]
    [ProducesResponseType(typeof(ApiResponse<LoadBalancerConfigResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLoadBalancerConfig(
        [FromBody] UpdateLoadBalancerConfigRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateLoadBalancerConfigCommand
            {
                Strategy = request.Strategy,
                StickySessionsEnabled = request.StickySessionsEnabled,
                StickySessionTtlMinutes = request.StickySessionTtlMinutes,
            },
            cancellationToken);

        return Ok(ApiResponse<LoadBalancerConfigResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists pod health metrics.
    /// </summary>
    [HttpGet("api/v1/orchestrator/health")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PodHealthMetricResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPodHealthMetrics(
        [FromQuery] Guid? podId,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ListPodHealthMetricsQuery { PodId = podId, Limit = limit },
            cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PodHealthMetricResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists scaling events.
    /// </summary>
    [HttpGet("api/v1/orchestrator/scaling-events")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ScalingEventResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListScalingEvents(
        [FromQuery] Guid? poolId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ListScalingEventsQuery { PoolId = poolId, Limit = limit },
            cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ScalingEventResponse>>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
