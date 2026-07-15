using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Security;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Organization security and governance policies.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/policies")]
[Produces("application/json")]
public sealed class PoliciesController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PoliciesController"/> class.
    /// </summary>
    public PoliciesController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets policies.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<OrganizationPoliciesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPoliciesQuery(), cancellationToken);
        return Ok(ApiResponse<OrganizationPoliciesResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates policies.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<OrganizationPoliciesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromBody] UpdatePoliciesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdatePoliciesCommand { Request = request }, cancellationToken);
        return Ok(ApiResponse<OrganizationPoliciesResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
