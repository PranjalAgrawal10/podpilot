using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Deployments;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Deployments;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Deployment catalog endpoints (GPUs, models, regions, templates).
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class DeploymentCatalogController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentCatalogController"/> class.
    /// </summary>
    public DeploymentCatalogController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists GPU catalog entries.
    /// </summary>
    [HttpGet("api/v1/gpus")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GpuCatalogResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListGpus(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListGpuCatalogQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GpuCatalogResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Recommends a GPU for selected models.
    /// </summary>
    [HttpPost("api/v1/gpus/recommend")]
    [ProducesResponseType(typeof(ApiResponse<GpuRecommendationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecommendGpu(
        [FromBody] RecommendGpuRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RecommendGpuQuery { Models = request.Models },
            cancellationToken);
        return Ok(ApiResponse<GpuRecommendationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists model catalog entries for deployments.
    /// </summary>
    [HttpGet("api/v1/models/catalog")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ModelCatalogResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListModelCatalog(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListModelCatalogQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ModelCatalogResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists regions for an org-scoped compute provider.
    /// </summary>
    [HttpGet("api/v1/regions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeploymentRegionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRegions(
        [FromQuery] Guid providerId,
        [FromQuery] string? sortBy,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListDeploymentRegionsQuery { ProviderId = providerId, SortBy = sortBy },
            cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DeploymentRegionResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists deployment templates.
    /// </summary>
    [HttpGet("api/v1/templates")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeploymentTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTemplates(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDeploymentTemplatesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DeploymentTemplateResponse>>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
