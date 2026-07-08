namespace PodPilot.Domain.Enums;

/// <summary>
/// Organization-scoped roles for multi-tenant authorization.
/// </summary>
public enum OrganizationRole
{
    /// <summary>
    /// Full control including ownership transfer and organization deletion.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Can manage members, invitations, and organization settings.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Can create and manage workloads but not users.
    /// </summary>
    Developer = 2,

    /// <summary>
    /// Read-only access to organization resources.
    /// </summary>
    Viewer = 3,
}
