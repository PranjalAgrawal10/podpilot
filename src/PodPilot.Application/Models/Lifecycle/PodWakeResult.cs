namespace PodPilot.Application.Models.Lifecycle;

/// <summary>
/// Result of a pod wake operation.
/// </summary>
public sealed class PodWakeResult
{
    /// <summary>
    /// Gets or sets whether the wake succeeded or was queued.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets whether the wake was queued for background processing.
    /// </summary>
    public bool Queued { get; init; }

    /// <summary>
    /// Gets or sets the wake request identifier when queued.
    /// </summary>
    public Guid? WakeRequestId { get; init; }

    /// <summary>
    /// Gets or sets the resulting pod status.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets or sets an error message when the operation fails.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
