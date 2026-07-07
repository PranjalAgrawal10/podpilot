namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an application user.
/// </summary>
public class User : Common.AuditableEntity
{
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
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the refresh tokens associated with this user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>
    /// Gets the organization memberships for this user.
    /// </summary>
    public ICollection<OrganizationMember> OrganizationMemberships { get; set; } = [];
}
