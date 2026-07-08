using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// API key for authenticating AI gateway requests.
/// </summary>
public class GatewayApiKey : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the owning user for personal keys.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the key type.
    /// </summary>
    public GatewayApiKeyType KeyType { get; set; }

    /// <summary>
    /// Gets or sets a display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key prefix for identification.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed key value.
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the key expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets when the key was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the key is revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Gets or sets requests allowed per minute.
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Gets or sets requests allowed per day.
    /// </summary>
    public int RateLimitPerDay { get; set; } = 10000;

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;
}
