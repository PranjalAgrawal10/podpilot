using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Billing, plans, subscription, usage, and invoices.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/billing")]
[Produces("application/json")]
public sealed class BillingController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillingController"/> class.
    /// </summary>
    public BillingController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Lists subscription plans.</summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlanResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPlans(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPlansQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PlanResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets the organization subscription.</summary>
    [HttpGet("subscription")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscription(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubscriptionQuery(), cancellationToken);
        return Ok(ApiResponse<SubscriptionResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Ensures subscription exists (idempotent get-or-create).</summary>
    [HttpPost("subscription")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnsureSubscription(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubscriptionQuery(), cancellationToken);
        return Ok(ApiResponse<SubscriptionResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Starts checkout for a plan upgrade.</summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<CheckoutSessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartCheckout(
        [FromBody] StartCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StartCheckoutCommand { Request = request }, cancellationToken);
        return Ok(ApiResponse<CheckoutSessionResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Cancels the organization subscription.</summary>
    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        [FromBody] CancelSubscriptionRequest? request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new CancelSubscriptionCommand { AtPeriodEnd = request?.AtPeriodEnd ?? true },
            cancellationToken);
        return NoContent();
    }

    /// <summary>Gets usage dashboard.</summary>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(ApiResponse<UsageResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUsageQuery(), cancellationToken);
        return Ok(ApiResponse<UsageResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists invoices.</summary>
    [HttpGet("invoices")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvoiceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListInvoices(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListInvoicesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InvoiceResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Generates an invoice for the current period.</summary>
    [HttpPost("invoices")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> GenerateInvoice(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GenerateInvoiceCommand(), cancellationToken);
        return CreatedAtAction(
            nameof(ListInvoices),
            ApiResponse<InvoiceResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
