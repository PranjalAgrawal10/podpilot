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

    /// <summary>
    /// Permission or role assignment changed.
    /// </summary>
    PermissionChanged = 6,

    /// <summary>
    /// Secret was accessed or rotated.
    /// </summary>
    SecretAccessed = 7,

    /// <summary>
    /// Security or governance policy changed.
    /// </summary>
    PolicyChanged = 8,

    /// <summary>
    /// MFA challenge succeeded or failed.
    /// </summary>
    MfaChallenge = 9,

    /// <summary>
    /// SSO authentication event.
    /// </summary>
    SsoLogin = 10,

    /// <summary>
    /// Compliance export or erasure.
    /// </summary>
    ComplianceAction = 11,

    /// <summary>
    /// Security alert generated.
    /// </summary>
    SecurityAlert = 12,
}
