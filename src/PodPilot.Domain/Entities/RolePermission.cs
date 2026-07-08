namespace PodPilot.Domain.Entities;

/// <summary>
/// Junction between roles and permissions.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Gets or sets the role identifier.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the permission identifier.
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Gets or sets the permission.
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
