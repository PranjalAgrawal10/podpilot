namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Wake operation response.
/// </summary>
public sealed class PodWakeResponse
{
    /// <summary>
    /// Gets or sets whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets whether the wake was queued.
    /// </summary>
    public bool Queued { get; init; }

    /// <summary>
    /// Gets or sets the wake request identifier.
    /// </summary>
    public Guid? WakeRequestId { get; init; }

    /// <summary>
    /// Gets or sets the pod status.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets or sets an error message.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
