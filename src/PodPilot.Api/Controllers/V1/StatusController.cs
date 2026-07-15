using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Public system status endpoint.
/// </summary>
[ApiController]
[Route("api/v1/status")]
[Produces("application/json")]
public sealed class StatusController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusController"/> class.
    /// </summary>
    public StatusController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets anonymous system status.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SystemStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSystemStatusQuery(), cancellationToken);
        return Ok(ApiResponse<SystemStatusResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
