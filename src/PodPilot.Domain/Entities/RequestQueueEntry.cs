using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// A queued scheduler request waiting for pod capacity.
/// </summary>
public class RequestQueueEntry : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the linked gateway request identifier.
    /// </summary>
    public Guid GatewayRequestId { get; set; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string QueueName { get; set; } = "default";

    /// <summary>
    /// Gets or sets the request priority.
    /// </summary>
    public RequestPriority Priority { get; set; } = RequestPriority.Normal;

    /// <summary>
    /// Gets or sets when the request was enqueued.
    /// </summary>
    public DateTime EnqueuedAt { get; set; }

    /// <summary>
    /// Gets or sets the queue position at enqueue time.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets whether the entry is still waiting.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the client request identifier for duplicate detection.
    /// </summary>
    public string? ClientRequestId { get; set; }
}
