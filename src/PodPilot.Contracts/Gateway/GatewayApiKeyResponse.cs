namespace PodPilot.Contracts.Gateway;

/// <summary>
/// Gateway API key response.
/// </summary>
public sealed class GatewayApiKeyResponse
{
    /// <summary>
    /// Gets or sets the key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key prefix.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key type.
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the key is revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Gets or sets optional expiration timestamp.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets requests allowed per minute.
    /// </summary>
    public int RateLimitPerMinute { get; set; }

    /// <summary>
    /// Gets or sets requests allowed per day.
    /// </summary>
    public int RateLimitPerDay { get; set; }

    /// <summary>
    /// Gets or sets the plaintext key returned only on create/rotate.
    /// </summary>
    public string? PlaintextKey { get; set; }

    /// <summary>
    /// Gets or sets when the key was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
