using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit record for a proxied gateway request.
/// </summary>
public class GatewayRequest : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the initiating user identifier.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the API key identifier.
    /// </summary>
    public Guid? ApiKeyId { get; set; }

    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the resolved model identifier.
    /// </summary>
    public Guid? ModelId { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved model name.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the request status.
    /// </summary>
    public GatewayRequestStatus Status { get; set; } = GatewayRequestStatus.Pending;

    /// <summary>
    /// Gets or sets the scheduler priority.
    /// </summary>
    public RequestPriority Priority { get; set; } = RequestPriority.Normal;

    /// <summary>
    /// Gets or sets a value indicating whether a wake was triggered.
    /// </summary>
    public bool WakeTriggered { get; set; }

    /// <summary>
    /// Gets or sets whether the response was streamed.
    /// </summary>
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Gets or sets when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the request started executing.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the request completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets queue wait time in milliseconds.
    /// </summary>
    public int? QueueTimeMs { get; set; }

    /// <summary>
    /// Gets or sets execution time in milliseconds.
    /// </summary>
    public int? ExecutionTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the client-provided request identifier.
    /// </summary>
    public string? ClientRequestId { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the upstream base URL.
    /// </summary>
    public string? UpstreamBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a SHA-256 hash of the request body for duplicate detection.
    /// </summary>
    public string? RequestBodyHash { get; set; }

    /// <summary>
    /// Gets latency metrics.
    /// </summary>
    public GatewayLatency? Latency { get; set; }

    /// <summary>
    /// Gets error details when failed.
    /// </summary>
    public GatewayError? Error { get; set; }
}
