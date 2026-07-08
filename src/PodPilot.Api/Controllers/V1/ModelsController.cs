using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Models.Commands.DeleteModel;
using PodPilot.Application.Models.Commands.PullModel;
using PodPilot.Application.Models.Commands.RefreshModels;
using PodPilot.Application.Models.Commands.SetDefaultModel;
using PodPilot.Application.Models.Queries.GetDownloads;
using PodPilot.Application.Models.Queries.GetHealth;
using PodPilot.Application.Models.Queries.GetModelDashboard;
using PodPilot.Application.Models.Queries.GetModelDetails;
using PodPilot.Application.Models.Queries.GetModels;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Models;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Ollama model management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/models")]
[Authorize]
[Produces("application/json")]
public sealed class ModelsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelsController"/> class.
    /// </summary>
    public ModelsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists AI models.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ModelResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListModels([FromQuery] Guid? podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetModelsQuery { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ModelResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets model dashboard metrics.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<ModelDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid? podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetModelDashboardQuery { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<ModelDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets model details.
    /// </summary>
    [HttpGet("{modelId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ModelDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModel(Guid modelId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetModelDetailsQuery { ModelId = modelId }, cancellationToken);
        return Ok(ApiResponse<ModelDetailResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Pulls a model to a pod.
    /// </summary>
    [HttpPost("pull")]
    [ProducesResponseType(typeof(ApiResponse<ModelDownloadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PullModel([FromBody] PullModelRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new PullModelCommand { PodId = request.PodId, Model = request.Model },
            cancellationToken);

        return Ok(ApiResponse<ModelDownloadResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Deletes a model.
    /// </summary>
    [HttpDelete("{modelId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteModel(
        Guid modelId,
        [FromQuery] bool forceDefault,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteModelCommand { ModelId = modelId, ForceDefault = forceDefault },
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(new { deleted = true }, GetCorrelationId()));
    }

    /// <summary>
    /// Sets the default model for a pod.
    /// </summary>
    [HttpPost("{modelId:guid}/default")]
    [ProducesResponseType(typeof(ApiResponse<ModelResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultModel(Guid modelId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetDefaultModelCommand { ModelId = modelId }, cancellationToken);
        return Ok(ApiResponse<ModelResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Refreshes models from Ollama.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ModelResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshModels([FromBody] RefreshModelsRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshModelsCommand { PodId = request.PodId }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ModelResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists model downloads.
    /// </summary>
    [HttpGet("downloads")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ModelDownloadResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDownloads([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetDownloadsQuery { ActiveOnly = activeOnly }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ModelDownloadResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists model health records.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ModelHealthResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListHealth(
        [FromQuery] Guid? modelId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetHealthQuery { ModelId = modelId, Limit = limit },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<ModelHealthResponse>>.Ok(result, GetCorrelationId()));
    }

    private string GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
}
