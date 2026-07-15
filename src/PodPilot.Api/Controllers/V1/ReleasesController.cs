using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Platform release status.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/releases")]
[Produces("application/json")]
public sealed class ReleasesController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleasesController"/> class.
    /// </summary>
    public ReleasesController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets release status.</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<ReleaseStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetReleaseStatusQuery(), cancellationToken);
        return Ok(ApiResponse<ReleaseStatusResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
