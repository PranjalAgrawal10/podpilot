using MediatR;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Scheduler.Commands.CancelSchedulerRequest;
using PodPilot.Application.Scheduler.Queries.GetQueueStatus;
using PodPilot.Application.Scheduler.Queries.GetSchedulerRequest;
using PodPilot.Application.Scheduler.Queries.GetSchedulerStatus;
using PodPilot.Application.Scheduler.Queries.ListSchedulerRequests;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Scheduler and request queue management endpoints.
/// </summary>
[ApiController]
[Route("api/v1")]
public sealed class SchedulerController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerController"/> class.
    /// </summary>
    public SchedulerController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists scheduler requests.
    /// </summary>
    [HttpGet("requests")]
    public async Task<IActionResult> ListRequests(
        [FromQuery] string? status,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ListSchedulerRequestsQuery { Status = status, Limit = limit },
            cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<Contracts.Scheduler.SchedulerRequestResponse>>
        {
            Success = true,
            Data = result,
        });
    }

    /// <summary>
    /// Gets a scheduler request by identifier.
    /// </summary>
    [HttpGet("requests/{requestId:guid}")]
    public async Task<IActionResult> GetRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSchedulerRequestQuery { RequestId = requestId }, cancellationToken);
        return Ok(new ApiResponse<Contracts.Scheduler.SchedulerRequestResponse>
        {
            Success = true,
            Data = result,
        });
    }

    /// <summary>
    /// Cancels a scheduler request.
    /// </summary>
    [HttpPost("requests/{requestId:guid}/cancel")]
    public async Task<IActionResult> CancelRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var cancelled = await mediator.Send(new CancelSchedulerRequestCommand { RequestId = requestId }, cancellationToken);
        return Ok(new ApiResponse<bool> { Success = cancelled, Data = cancelled });
    }

    /// <summary>
    /// Gets queue metrics.
    /// </summary>
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetQueueStatusQuery(), cancellationToken);
        return Ok(new ApiResponse<Contracts.Scheduler.QueueStatusResponse>
        {
            Success = true,
            Data = result,
        });
    }

    /// <summary>
    /// Gets scheduler status.
    /// </summary>
    [HttpGet("scheduler/status")]
    public async Task<IActionResult> GetSchedulerStatus(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSchedulerStatusQuery(), cancellationToken);
        return Ok(new ApiResponse<Contracts.Scheduler.SchedulerStatusResponse>
        {
            Success = true,
            Data = result,
        });
    }
}
