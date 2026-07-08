namespace PodPilot.Contracts.Scheduler;

/// <summary>
/// Scheduler request summary.
/// </summary>
public sealed class SchedulerRequestResponse
{
    /// <summary>Gets or sets the request identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the model name.</summary>
    public string? Model { get; init; }

    /// <summary>Gets or sets the HTTP path.</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Gets or sets the status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets or sets the priority.</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets or sets when the request was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets when execution started.</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>Gets or sets when the request completed.</summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>Gets or sets queue wait time in milliseconds.</summary>
    public int? QueueTimeMs { get; init; }

    /// <summary>Gets or sets execution time in milliseconds.</summary>
    public int? ExecutionTimeMs { get; init; }

    /// <summary>Gets or sets the retry count.</summary>
    public int RetryCount { get; init; }

    /// <summary>Gets or sets whether the response was streamed.</summary>
    public bool IsStreaming { get; init; }
}
