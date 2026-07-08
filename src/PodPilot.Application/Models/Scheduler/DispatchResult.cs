namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Result of dispatching a request.
/// </summary>
public sealed class DispatchResult
{
    /// <summary>
    /// Gets or sets a value indicating whether dispatch succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets whether the response was streamed.
    /// </summary>
    public bool IsStreaming { get; init; }

    /// <summary>
    /// Gets or sets response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets forward latency in milliseconds.
    /// </summary>
    public int ForwardLatencyMs { get; init; }

    /// <summary>
    /// Gets or sets whether the failure is retryable.
    /// </summary>
    public bool IsRetryable { get; init; }

    /// <summary>
    /// Gets or sets an error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets wake latency in milliseconds.
    /// </summary>
    public int? WakeLatencyMs { get; init; }

    /// <summary>
    /// Gets or sets health check latency in milliseconds.
    /// </summary>
    public int? HealthCheckLatencyMs { get; init; }
}
