using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Commercial / billing dashboard.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/commercial")]
[Produces("application/json")]
public sealed class CommercialController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommercialController"/> class.
    /// </summary>
    public CommercialController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets the commercial dashboard.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<CommercialDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommercialDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<CommercialDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
