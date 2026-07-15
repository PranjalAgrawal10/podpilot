using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Security;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Enterprise audit endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/audit")]
[Produces("application/json")]
public sealed class AuditController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditController"/> class.
    /// </summary>
    public AuditController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Lists audit events.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AuditEventResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? category,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ListAuditEventsQuery
            {
                Category = category,
                EventType = eventType,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Take = take,
            },
            cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AuditEventResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets an audit event.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAuditEventQuery { AuditEventId = id }, cancellationToken);
        return Ok(ApiResponse<AuditEventResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
