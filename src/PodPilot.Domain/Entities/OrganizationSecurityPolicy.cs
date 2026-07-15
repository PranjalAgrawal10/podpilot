namespace PodPilot.Domain.Entities;

/// <summary>
/// Organization security controls (password, MFA, sessions, IP/geo).
/// </summary>
public class OrganizationSecurityPolicy : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets minimum password length.</summary>
    public int MinPasswordLength { get; set; } = 12;

    /// <summary>Gets or sets whether passwords require uppercase letters.</summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>Gets or sets whether passwords require digits.</summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>Gets or sets whether passwords require non-alphanumeric characters.</summary>
    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>Gets or sets whether MFA is required for all members.</summary>
    public bool RequireMfa { get; set; }

    /// <summary>Gets or sets session timeout in minutes.</summary>
    public int SessionTimeoutMinutes { get; set; } = 480;

    /// <summary>Gets or sets maximum concurrent sessions per user (0 = unlimited).</summary>
    public int MaxConcurrentSessions { get; set; } = 5;

    /// <summary>Gets or sets IP allow-list JSON array (empty = all).</summary>
    public string IpAllowListJson { get; set; } = "[]";

    /// <summary>Gets or sets allowed country codes JSON array (empty = all).</summary>
    public string GeoAllowListJson { get; set; } = "[]";

    /// <summary>Gets or sets default API key lifetime in days.</summary>
    public int ApiKeyExpirationDays { get; set; } = 90;

    /// <summary>Gets or sets whether API key rotation reminders are enforced.</summary>
    public bool EnforceApiKeyRotation { get; set; } = true;

    /// <summary>Gets or sets failed login threshold before alert.</summary>
    public int FailedLoginAlertThreshold { get; set; } = 5;

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;
}
