using MediatR;
using PodPilot.Contracts.Auth;

namespace PodPilot.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token using a refresh token.
/// </summary>
public sealed class RefreshTokenCommand : IRequest<AuthResponse>
{
    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
