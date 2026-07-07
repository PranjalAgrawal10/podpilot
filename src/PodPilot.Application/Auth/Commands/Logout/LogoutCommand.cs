using MediatR;

namespace PodPilot.Application.Auth.Commands.Logout;

/// <summary>
/// Command to log out a user by revoking their refresh token.
/// </summary>
public sealed class LogoutCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the refresh token to revoke.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
