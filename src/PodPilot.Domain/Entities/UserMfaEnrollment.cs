namespace PodPilot.Domain.Entities;

/// <summary>
/// TOTP MFA enrollment for a user (secret encrypted at rest).
/// </summary>
public class UserMfaEnrollment : Common.AuditableEntity
{
    /// <summary>Gets or sets the user identifier.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets whether MFA is enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets the encrypted TOTP shared secret.</summary>
    public string? EncryptedTotpSecret { get; set; }

    /// <summary>Gets or sets when MFA was enabled.</summary>
    public DateTime? EnabledAt { get; set; }

    /// <summary>Gets or sets encrypted recovery codes JSON.</summary>
    public string? EncryptedRecoveryCodesJson { get; set; }
}
