namespace PodPilot.Contracts.Gateway;

/// <summary>
/// Request to create a gateway API key.
/// </summary>
public sealed class CreateGatewayApiKeyRequest
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a personal key.
    /// </summary>
    public bool IsPersonal { get; set; }

    /// <summary>
    /// Gets or sets optional expiration timestamp (UTC).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets requests allowed per minute.
    /// </summary>
    public int? RateLimitPerMinute { get; set; }

    /// <summary>
    /// Gets or sets requests allowed per day.
    /// </summary>
    public int? RateLimitPerDay { get; set; }
}
