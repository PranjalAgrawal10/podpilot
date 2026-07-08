namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Result of enqueueing a request.
/// </summary>
public sealed class EnqueueResult
{
    /// <summary>
    /// Gets or sets a value indicating whether enqueue succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the queue position.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Gets or sets an error message when enqueue fails.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
