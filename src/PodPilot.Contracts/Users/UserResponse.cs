namespace PodPilot.Contracts.Users;

/// <summary>
/// Response model for the current authenticated user.
/// </summary>
public sealed class UserResponse
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
    /// Gets or sets the organizations the user belongs to.
    /// </summary>
    public IReadOnlyList<OrganizationSummary> Organizations { get; set; } = [];
}

/// <summary>
/// Summary organization information for a user.
/// </summary>
public sealed class OrganizationSummary
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role in the organization.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
