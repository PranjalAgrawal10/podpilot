namespace PodPilot.Contracts.Gateway;

/// <summary>
/// Summary of a gateway request for dashboard display.
/// </summary>
public sealed class GatewayRequestSummaryResponse
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the request status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether a wake was triggered.
    /// </summary>
    public bool WakeTriggered { get; set; }

    /// <summary>
    /// Gets or sets whether the response was streamed.
    /// </summary>
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Gets or sets total latency in milliseconds.
    /// </summary>
    public int? TotalLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets when the request started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the request completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
