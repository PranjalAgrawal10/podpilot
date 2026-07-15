using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Security;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Security dashboard, sessions, devices, and identity provider management.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/security")]
[Produces("application/json")]
public sealed class SecurityController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityController"/> class.
    /// </summary>
    public SecurityController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Gets the security dashboard.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<SecurityDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSecurityDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<SecurityDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists active sessions.</summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SessionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSessionsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SessionResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists trusted devices.</summary>
    [HttpGet("devices")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TrustedDeviceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Devices(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTrustedDevicesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TrustedDeviceResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Revokes a trusted device.</summary>
    [HttpDelete("devices/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeDevice(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeTrustedDeviceCommand { DeviceId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Lists identity providers.</summary>
    [HttpGet("identity-providers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IdentityProviderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListIdentityProviders(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListIdentityProvidersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IdentityProviderResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Creates an identity provider.</summary>
    [HttpPost("identity-providers")]
    [ProducesResponseType(typeof(ApiResponse<IdentityProviderResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateIdentityProvider(
        [FromBody] CreateIdentityProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateIdentityProviderCommand { Request = request }, cancellationToken);
        return CreatedAtAction(
            nameof(ListIdentityProviders),
            ApiResponse<IdentityProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Deletes an identity provider.</summary>
    [HttpDelete("identity-providers/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteIdentityProvider(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteIdentityProviderCommand { IdentityProviderId = id }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
