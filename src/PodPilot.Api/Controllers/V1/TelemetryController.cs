using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Telemetry preference endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/telemetry")]
[Produces("application/json")]
public sealed class TelemetryController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryController"/> class.
    /// </summary>
    public TelemetryController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets telemetry preference.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TelemetryPreferenceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTelemetryQuery(), cancellationToken);
        return Ok(ApiResponse<TelemetryPreferenceResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates telemetry preference.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<TelemetryPreferenceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromBody] TelemetryPreferenceResponse preference,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTelemetryCommand { Preference = preference },
            cancellationToken);
        return Ok(ApiResponse<TelemetryPreferenceResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
