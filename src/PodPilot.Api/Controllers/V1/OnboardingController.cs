using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Onboarding wizard endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/onboarding")]
[Produces("application/json")]
public sealed class OnboardingController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardingController"/> class.
    /// </summary>
    public OnboardingController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets onboarding status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<OnboardingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOnboardingQuery(), cancellationToken);
        return Ok(ApiResponse<OnboardingResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Marks an onboarding step complete.</summary>
    [HttpPost("steps/complete")]
    [ProducesResponseType(typeof(ApiResponse<OnboardingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteStep(
        [FromBody] CompleteOnboardingStepRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteOnboardingStepCommand { Step = request.Step },
            cancellationToken);
        return Ok(ApiResponse<OnboardingResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Dismisses onboarding.</summary>
    [HttpPost("dismiss")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Dismiss(CancellationToken cancellationToken)
    {
        await mediator.Send(new DismissOnboardingCommand(), cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
