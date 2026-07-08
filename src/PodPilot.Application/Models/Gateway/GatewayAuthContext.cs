namespace PodPilot.Application.Models.Gateway;

/// <summary>
/// Authenticated gateway context resolved from an API key.
/// </summary>
public sealed class GatewayAuthContext
{
    /// <summary>
    /// Gets or sets the API key identifier.
    /// </summary>
    public Guid ApiKeyId { get; init; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; init; }

    /// <summary>
    /// Gets or sets the user identifier for personal keys.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Gets or sets requests allowed per minute.
    /// </summary>
    public int RateLimitPerMinute { get; init; }

    /// <summary>
    /// Gets or sets requests allowed per day.
    /// </summary>
    public int RateLimitPerDay { get; init; }
}
