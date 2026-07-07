using MediatR;
using PodPilot.Contracts.Auth;

namespace PodPilot.Application.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user.
/// </summary>
public sealed class LoginCommand : IRequest<AuthResponse>
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
