namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Pod lifecycle event response.
/// </summary>
public sealed class PodLifecycleEventResponse
{
    /// <summary>
    /// Gets or sets the event identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the event source.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Gets or sets the event message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets or sets optional metadata.
    /// </summary>
    public string? Metadata { get; init; }
}
