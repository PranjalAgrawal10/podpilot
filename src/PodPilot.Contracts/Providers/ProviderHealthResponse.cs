namespace PodPilot.Contracts.Providers;

/// <summary>
/// Provider health response.
/// </summary>
public sealed class ProviderHealthResponse
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets when health was last checked.
    /// </summary>
    public DateTime? LastCheckedAt { get; init; }

    /// <summary>
    /// Gets or sets the response time in milliseconds.
    /// </summary>
    public int? ResponseTimeMs { get; init; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
