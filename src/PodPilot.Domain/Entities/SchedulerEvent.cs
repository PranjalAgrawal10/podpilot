using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit event emitted by the request scheduler.
/// </summary>
public class SchedulerEvent : Common.BaseEntity
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
    /// Gets or sets the event type.
    /// </summary>
    public SchedulerEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the event message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets optional JSON metadata.
    /// </summary>
    public string? Metadata { get; set; }
}
