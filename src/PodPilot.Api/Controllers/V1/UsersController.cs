using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Users.Queries.GetCurrentUser;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Users;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// User management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public UsersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current user profile.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
