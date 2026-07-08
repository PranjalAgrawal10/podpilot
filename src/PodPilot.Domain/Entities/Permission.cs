namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a granular permission within the platform.
/// </summary>
public class Permission : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the unique permission name (e.g. Organization.Read).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the permission description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the permission category for grouping.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets role assignments for this permission.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
