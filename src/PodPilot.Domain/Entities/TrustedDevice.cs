namespace PodPilot.Domain.Entities;

/// <summary>
/// Device trusted by a user for reduced MFA friction / tracking.
/// </summary>
public class TrustedDevice : Common.AuditableEntity
{
    /// <summary>Gets or sets the user identifier.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the organization identifier when scoped.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets the device label.</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the fingerprint hash.</summary>
    public string FingerprintHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the last seen IP.</summary>
    public string? LastIpAddress { get; set; }

    /// <summary>Gets or sets the last user agent.</summary>
    public string? LastUserAgent { get; set; }

    /// <summary>Gets or sets when the device was trusted.</summary>
    public DateTime TrustedAt { get; set; }

    /// <summary>Gets or sets the last seen timestamp.</summary>
    public DateTime LastSeenAt { get; set; }

    /// <summary>Gets or sets when trust expires.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Gets or sets whether the device is revoked.</summary>
    public bool IsRevoked { get; set; }
}
