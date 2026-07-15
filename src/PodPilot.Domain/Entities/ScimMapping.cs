namespace PodPilot.Domain.Entities;

/// <summary>
/// Maps an external SCIM subject/group to a PodPilot organization role.
/// </summary>
public class ScimMapping : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the identity provider identifier.</summary>
    public Guid? IdentityProviderId { get; set; }

    /// <summary>Gets or sets the external group or directory identifier.</summary>
    public string ExternalGroupId { get; set; } = string.Empty;

    /// <summary>Gets or sets the external group display name.</summary>
    public string? ExternalGroupName { get; set; }

    /// <summary>Gets or sets the mapped organization role name.</summary>
    public string OrganizationRole { get; set; } = "Viewer";

    /// <summary>Gets or sets whether the mapping is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>Gets the optional identity provider.</summary>
    public IdentityProvider? IdentityProvider { get; set; }
}
