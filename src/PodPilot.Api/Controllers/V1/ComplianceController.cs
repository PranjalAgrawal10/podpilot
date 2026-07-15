using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Models.Security;
using PodPilot.Application.Security;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Compliance status, export, and erasure endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/compliance")]
[Produces("application/json")]
public sealed class ComplianceController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComplianceController"/> class.
    /// </summary>
    public ComplianceController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets compliance status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetComplianceQuery(), cancellationToken);
        return Ok(ApiResponse<ComplianceStatusResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Exports compliance data.</summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(ApiResponse<ComplianceExportResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportComplianceCommand(), cancellationToken);
        return Ok(ApiResponse<ComplianceExportResult>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Performs right-to-erasure for a user.</summary>
    [HttpPost("erasure")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Erasure(
        [FromBody] ErasureRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new EraseUserComplianceCommand { TargetUserId = request.TargetUserId }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}

/// <summary>Erasure request body.</summary>
public sealed class ErasureRequest
{
    /// <summary>Gets or sets the target user id.</summary>
    public Guid TargetUserId { get; set; }
}
