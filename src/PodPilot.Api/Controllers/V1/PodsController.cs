using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Pods.Commands.CreatePod;
using PodPilot.Application.Pods.Commands.DeletePod;
using PodPilot.Application.Pods.Commands.RestartPod;
using PodPilot.Application.Pods.Commands.StartPod;
using PodPilot.Application.Pods.Commands.StopPod;
using PodPilot.Application.Pods.Commands.SyncPod;
using PodPilot.Application.Pods.Commands.UpdatePod;
using PodPilot.Application.Pods.Queries.GetPod;
using PodPilot.Application.Pods.Queries.ListPods;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// GPU pod management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/pods")]
[Authorize]
[Produces("application/json")]
public sealed class PodsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public PodsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists GPU pods for the current organization.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PodResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPods(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPodsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PodResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets a GPU pod by identifier.
    /// </summary>
    [HttpGet("{podId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPod(Guid podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPodQuery { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a GPU pod.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePod(
        [FromBody] CreatePodRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<GpuType>(request.GpuType, ignoreCase: true, out var gpuType))
        {
            throw new ValidationException($"GPU type '{request.GpuType}' is invalid.");
        }

        var result = await mediator.Send(
            new CreatePodCommand
            {
                ProviderId = request.ProviderId,
                Name = request.Name,
                Description = request.Description,
                GpuId = request.GpuId,
                GpuType = gpuType,
                Region = request.Region,
                TemplateId = request.TemplateId,
                TemplateName = request.TemplateName,
                ImageName = request.ImageName,
                ContainerDiskGb = request.ContainerDiskGb,
                VolumeDiskGb = request.VolumeDiskGb,
                VolumeMountPath = request.VolumeMountPath,
                GpuCount = request.GpuCount,
                EnvironmentVariables = request.EnvironmentVariables ?? new Dictionary<string, string>(),
                Ports = request.Ports ?? [],
                EnablePublicIp = request.EnablePublicIp,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetPod),
            new { podId = result.Id },
            ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Updates a GPU pod.
    /// </summary>
    [HttpPut("{podId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePod(
        Guid podId,
        [FromBody] UpdatePodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdatePodCommand
            {
                PodId = podId,
                Name = request.Name,
                Description = request.Description,
            },
            cancellationToken);

        return Ok(ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Deletes a GPU pod.
    /// </summary>
    [HttpDelete("{podId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePod(
        Guid podId,
        [FromBody] DeletePodRequest? request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeletePodCommand
            {
                PodId = podId,
                Force = request?.Force ?? false,
            },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Starts a GPU pod.
    /// </summary>
    [HttpPost("{podId:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartPod(Guid podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StartPodCommand { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Stops a GPU pod.
    /// </summary>
    [HttpPost("{podId:guid}/stop")]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StopPod(Guid podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StopPodCommand { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Restarts a GPU pod.
    /// </summary>
    [HttpPost("{podId:guid}/restart")]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RestartPod(Guid podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestartPodCommand { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Synchronizes a pod's status with the provider.
    /// </summary>
    [HttpPost("{podId:guid}/sync")]
    [ProducesResponseType(typeof(ApiResponse<PodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncPod(Guid podId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SyncPodCommand { PodId = podId }, cancellationToken);
        return Ok(ApiResponse<PodResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
