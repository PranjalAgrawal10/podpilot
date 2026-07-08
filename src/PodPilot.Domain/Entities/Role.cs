using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an organization role definition with associated permissions.
/// </summary>
public class Role : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization role enum value.
    /// </summary>
    public OrganizationRole OrganizationRole { get; set; }

    /// <summary>
    /// Gets permission assignments for this role.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
