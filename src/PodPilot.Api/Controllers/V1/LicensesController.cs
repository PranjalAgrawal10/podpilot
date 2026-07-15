using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Product license activation and issuance.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/licenses")]
[Produces("application/json")]
public sealed class LicensesController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicensesController"/> class.
    /// </summary>
    public LicensesController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets the active license.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLicenseQuery(), cancellationToken);
        return Ok(ApiResponse<LicenseResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Activates a license key.</summary>
    [HttpPost("activate")]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(
        [FromBody] ActivateLicenseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ActivateLicenseCommand { LicenseKey = request.LicenseKey },
            cancellationToken);
        return Ok(ApiResponse<LicenseResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Issues a new license key.</summary>
    [HttpPost("issue")]
    [ProducesResponseType(typeof(ApiResponse<IssuedLicenseResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Issue(
        [FromBody] IssueLicenseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IssueLicenseCommand { Request = request }, cancellationToken);
        return CreatedAtAction(nameof(Get), ApiResponse<IssuedLicenseResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
