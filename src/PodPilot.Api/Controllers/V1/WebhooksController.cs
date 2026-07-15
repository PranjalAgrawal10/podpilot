using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Payment provider webhooks.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/billing/webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhooksController"/> class.
    /// </summary>
    public WebhooksController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Handles Stripe webhooks.</summary>
    [HttpPost("stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Stripe(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        await mediator.Send(
            new HandleStripeWebhookCommand { Payload = payload, SignatureHeader = signature },
            cancellationToken);
        return Ok();
    }

    /// <summary>Handles Razorpay webhooks.</summary>
    [HttpPost("razorpay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Razorpay(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
        await mediator.Send(
            new HandleRazorpayWebhookCommand { Payload = payload, SignatureHeader = signature },
            cancellationToken);
        return Ok();
    }
}
