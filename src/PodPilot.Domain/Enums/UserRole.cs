namespace PodPilot.Domain.Enums;

/// <summary>
/// Application roles for authorization.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard organization member.
    /// </summary>
    Member = 0,

    /// <summary>
    /// Organization administrator with elevated privileges.
    /// </summary>
    Admin = 1,
}
