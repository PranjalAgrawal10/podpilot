using MediatR;
using PodPilot.Contracts.Auth;

namespace PodPilot.Application.Auth.Commands.Register;

/// <summary>
/// Command to register a new user and organization.
/// </summary>
public sealed class RegisterCommand : IRequest<AuthResponse>
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
}
