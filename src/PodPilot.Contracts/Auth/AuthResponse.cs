namespace PodPilot.Contracts.Auth;

/// <summary>
/// Authentication response containing tokens and user metadata.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token expiration in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the authenticated user.
    /// </summary>
    public UserSummary User { get; set; } = new();
}

/// <summary>
/// Summary user information returned in auth responses.
/// </summary>
public sealed class UserSummary
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's roles.
    /// </summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the current organization identifier from the token.
    /// </summary>
    public Guid? CurrentOrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the current organization role from the token.
    /// </summary>
    public string? CurrentOrganizationRole { get; set; }
}
