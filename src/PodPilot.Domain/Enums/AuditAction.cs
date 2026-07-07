namespace PodPilot.Domain.Enums;

/// <summary>
/// Types of auditable actions performed in the system.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Entity was created.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Entity was updated.
    /// </summary>
    Updated = 1,

    /// <summary>
    /// Entity was deleted.
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// User authenticated successfully.
    /// </summary>
    Login = 3,

    /// <summary>
    /// User logged out.
    /// </summary>
    Logout = 4,

    /// <summary>
    /// User registered a new account.
    /// </summary>
    Register = 5,
}
