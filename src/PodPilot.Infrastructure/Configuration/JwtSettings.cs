namespace PodPilot.Infrastructure.Configuration;

/// <summary>
/// JWT authentication settings.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signing key.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token lifetime in minutes.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
