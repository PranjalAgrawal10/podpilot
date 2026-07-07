using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Auth.Commands.Login;
using PodPilot.Application.Auth.Commands.Logout;
using PodPilot.Application.Auth.Commands.RefreshToken;
using PodPilot.Application.Auth.Commands.Register;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;

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
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens and user summary.</returns>
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
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens and user summary.</returns>
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
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>New authentication tokens.</returns>
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
    /// <param name="request">The refresh token to revoke.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
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

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
