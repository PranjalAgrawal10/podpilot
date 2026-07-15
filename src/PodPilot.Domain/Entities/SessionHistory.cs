namespace PodPilot.Domain.Entities;

/// <summary>
/// Record of an authenticated session for device/session governance.
/// </summary>
public class SessionHistory : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier when known.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets the user identifier.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the session identifier (refresh token family or JWT jti).</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>Gets or sets the IP address.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Gets or sets the user agent.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Gets or sets the device fingerprint hash.</summary>
    public string? DeviceFingerprint { get; set; }

    /// <summary>Gets or sets the country code if resolved.</summary>
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets when the session started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets the last activity timestamp.</summary>
    public DateTime LastSeenAt { get; set; }

    /// <summary>Gets or sets when the session ended.</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Gets or sets whether the session is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether login succeeded.</summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>Gets or sets a failure reason when applicable.</summary>
    public string? FailureReason { get; set; }
}
