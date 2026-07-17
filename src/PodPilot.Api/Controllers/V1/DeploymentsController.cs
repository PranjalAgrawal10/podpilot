using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Deployments;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Deployments;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// One-click AI pod deployment endpoints.
/// </summary>
[ApiController]
[Route("api/v1/deployments")]
[Authorize]
[Produces("application/json")]
public sealed class DeploymentsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentsController"/> class.
    /// </summary>
    public DeploymentsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists deployments for the current organization.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeploymentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDeploymentsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DeploymentResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets deployment dashboard aggregates.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DeploymentDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDeploymentDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<DeploymentDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets a deployment by id.
    /// </summary>
    [HttpGet("{deploymentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeploymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid deploymentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDeploymentQuery { DeploymentId = deploymentId }, cancellationToken);
        return Ok(ApiResponse<DeploymentResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a one-click deployment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DeploymentResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateDeploymentCommand { Request = request }, cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { deploymentId = result.Id },
            ApiResponse<DeploymentResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Requests deletion of a deployment.
    /// </summary>
    [HttpDelete("{deploymentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid deploymentId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteDeploymentCommand { DeploymentId = deploymentId }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Restarts a failed or ready deployment.
    /// </summary>
    [HttpPost("{deploymentId:guid}/restart")]
    [ProducesResponseType(typeof(ApiResponse<DeploymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Restart(Guid deploymentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestartDeploymentCommand { DeploymentId = deploymentId }, cancellationToken);
        return Ok(ApiResponse<DeploymentResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Runs an immediate health check.
    /// </summary>
    [HttpPost("{deploymentId:guid}/health")]
    [ProducesResponseType(typeof(ApiResponse<DeploymentHealthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Health(Guid deploymentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RunDeploymentHealthCommand { DeploymentId = deploymentId },
            cancellationToken);
        return Ok(ApiResponse<DeploymentHealthResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
