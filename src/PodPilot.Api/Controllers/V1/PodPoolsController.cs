using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Orchestration.Commands.CreatePodPool;
using PodPilot.Application.Orchestration.Commands.DeletePodPool;
using PodPilot.Application.Orchestration.Commands.UpdatePodPool;
using PodPilot.Application.Orchestration.Queries.GetPodPool;
using PodPilot.Application.Orchestration.Queries.ListPodPools;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Pod pool management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/pod-pools")]
[Authorize]
[Produces("application/json")]
public sealed class PodPoolsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodPoolsController"/> class.
    /// </summary>
    public PodPoolsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists pod pools for the current organization.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PodPoolResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPodPools(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPodPoolsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PodPoolResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets a pod pool by identifier.
    /// </summary>
    [HttpGet("{poolId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PodPoolResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPodPool(Guid poolId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPodPoolQuery { PoolId = poolId }, cancellationToken);
        return Ok(ApiResponse<PodPoolResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a pod pool.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PodPoolResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePodPool(
        [FromBody] CreatePodPoolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreatePodPoolCommand
            {
                Name = request.Name,
                Description = request.Description,
                PoolType = request.PoolType,
                IsDefault = request.IsDefault,
                ProviderId = request.ProviderId,
                GpuId = request.GpuId,
                GpuType = request.GpuType,
                Region = request.Region,
                TemplateId = request.TemplateId,
                ImageName = request.ImageName,
                Models = request.Models,
                PodIds = request.PodIds,
                ScalingPolicy = request.ScalingPolicy,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetPodPool),
            new { poolId = result.Id },
            ApiResponse<PodPoolResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Updates a pod pool.
    /// </summary>
    [HttpPut("{poolId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PodPoolResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePodPool(
        Guid poolId,
        [FromBody] UpdatePodPoolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdatePodPoolCommand
            {
                PoolId = poolId,
                Name = request.Name,
                Description = request.Description,
                PoolType = request.PoolType,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                Models = request.Models,
                PodIds = request.PodIds,
                ScalingPolicy = request.ScalingPolicy,
            },
            cancellationToken);

        return Ok(ApiResponse<PodPoolResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Deletes a pod pool.
    /// </summary>
    [HttpDelete("{poolId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePodPool(Guid poolId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePodPoolCommand { PoolId = poolId }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
