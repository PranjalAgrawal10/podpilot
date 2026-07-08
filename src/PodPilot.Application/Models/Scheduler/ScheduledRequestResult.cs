namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Result of scheduling and processing a request.
/// </summary>
public sealed class ScheduledRequestResult
{
    /// <summary>
    /// Gets or sets a value indicating whether processing succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public Guid? RequestId { get; init; }

    /// <summary>
    /// Gets or sets whether the request was queued.
    /// </summary>
    public bool WasQueued { get; init; }

    /// <summary>
    /// Gets or sets an error code.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets or sets an error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets response headers when successful.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
