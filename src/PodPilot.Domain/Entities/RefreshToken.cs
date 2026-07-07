namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a refresh token used for JWT rotation.
/// </summary>
public class RefreshToken : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the token value.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC expiration timestamp.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC revocation timestamp.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the token that replaced this one during rotation.
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Gets or sets the reason for revocation.
    /// </summary>
    public string? ReasonRevoked { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the associated user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the token is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Gets a value indicating whether the token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Gets a value indicating whether the token is active.
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;
}
