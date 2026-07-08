using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Item stored in the request queue.
/// </summary>
public sealed class QueuedRequestItem
{
    /// <summary>
    /// Gets or sets the gateway request identifier.
    /// </summary>
    public Guid RequestId { get; init; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; init; }

    /// <summary>
    /// Gets or sets the assigned pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets the request priority.
    /// </summary>
    public RequestPriority Priority { get; init; }

    /// <summary>
    /// Gets or sets when the item was enqueued.
    /// </summary>
    public DateTime EnqueuedAt { get; init; }

    /// <summary>
    /// Gets or sets the client request identifier.
    /// </summary>
    public string? ClientRequestId { get; init; }

    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string QueueName { get; init; } = "default";
}
