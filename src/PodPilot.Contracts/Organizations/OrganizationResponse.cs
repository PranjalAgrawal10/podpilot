namespace PodPilot.Contracts.Organizations;

/// <summary>
/// Organization details response.
/// </summary>
public sealed class OrganizationResponse
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
    /// Gets or sets the URL-friendly slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional logo URL.
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// Gets or sets the owner user identifier.
    /// </summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the user's default organization.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the organization is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the organization was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the current user's role in this organization.
    /// </summary>
    public string? CurrentUserRole { get; set; }
}
