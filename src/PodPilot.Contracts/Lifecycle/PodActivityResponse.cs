namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Pod activity response.
/// </summary>
public sealed class PodActivityResponse
{
    /// <summary>
    /// Gets or sets the activity identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    public string ActivityType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets when the activity occurred.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the activity source.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Gets or sets optional metadata.
    /// </summary>
    public string? Metadata { get; init; }
}
