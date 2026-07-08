namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an organization that groups users and resources.
/// </summary>
public class Organization : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL-friendly slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the organization logo URL.
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
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the members of this organization.
    /// </summary>
    public ICollection<OrganizationMember> Members { get; set; } = [];

    /// <summary>
    /// Gets the invitations for this organization.
    /// </summary>
    public ICollection<Invitation> Invitations { get; set; } = [];

    /// <summary>
    /// Gets the compute providers for this organization.
    /// </summary>
    public ICollection<ComputeProvider> ComputeProviders { get; set; } = [];
}
