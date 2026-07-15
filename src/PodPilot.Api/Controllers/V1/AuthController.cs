using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Auth.Commands.Login;
using PodPilot.Application.Auth.Commands.Logout;
using PodPilot.Application.Auth.Commands.RefreshToken;
using PodPilot.Application.Auth.Commands.Register;
using PodPilot.Application.Security;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Authentication endpoints.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public AuthController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Registers a new user and organization.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                OrganizationName = request.OrganizationName,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(Register),
            ApiResponse<AuthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginCommand
            {
                Email = request.Email,
                Password = request.Password,
            },
            cancellationToken);

        return Ok(ApiResponse<AuthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists SSO identity providers for an organization (public catalog).
    /// </summary>
    [HttpGet("providers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IdentityProviderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProviders(
        [FromQuery] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListIdentityProvidersQuery
            {
                OrganizationId = organizationId,
                PublicCatalog = true,
            },
            cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IdentityProviderResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Begins an SSO challenge.
    /// </summary>
    [HttpPost("sso/begin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SsoChallengeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BeginSso(
        [FromBody] BeginSsoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new BeginSsoCommand
            {
                OrganizationId = request.OrganizationId,
                IdentityProviderId = request.IdentityProviderId,
                RedirectUri = request.RedirectUri,
            },
            cancellationToken);
        return Ok(ApiResponse<SsoChallengeResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Completes an SSO login.
    /// </summary>
    [HttpPost("sso/complete")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteSso(
        [FromBody] CompleteSsoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteSsoCommand
            {
                OrganizationId = request.OrganizationId,
                IdentityProviderId = request.IdentityProviderId,
                Code = request.Code,
                State = request.State,
                SamlResponse = request.SamlResponse,
                RedirectUri = request.RedirectUri,
            },
            cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Enrolls, confirms, or verifies MFA.
    /// </summary>
    [HttpPost("mfa")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mfa(
        [FromBody] MfaRequest request,
        CancellationToken cancellationToken)
    {
        var action = (request.Action ?? "verify").Trim().ToLowerInvariant();
        return action switch
        {
            "enroll" => Ok(ApiResponse<MfaEnrollmentResponse>.Ok(
                await mediator.Send(new EnrollMfaCommand(), cancellationToken),
                GetCorrelationId())),
            "confirm" => await ConfirmMfaAsync(request, cancellationToken),
            _ => Ok(ApiResponse<AuthResponse>.Ok(
                await mediator.Send(
                    new VerifyMfaCommand
                    {
                        Code = request.Code ?? string.Empty,
                        MfaToken = request.MfaToken ?? string.Empty,
                    },
                    cancellationToken),
                GetCorrelationId())),
        };
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RefreshTokenCommand { RefreshToken = request.RefreshToken },
            cancellationToken);

        return Ok(ApiResponse<AuthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new LogoutCommand { RefreshToken = request.RefreshToken },
            cancellationToken);

        return NoContent();
    }

    private async Task<IActionResult> ConfirmMfaAsync(MfaRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ConfirmMfaCommand { Code = request.Code ?? string.Empty }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
