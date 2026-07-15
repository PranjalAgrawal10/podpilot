using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Immutable enterprise audit event (append-only).
/// </summary>
public class AuditEvent : Common.BaseEntity
{
    /// <summary>Gets or sets the organization identifier when applicable.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets the acting user identifier.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Gets or sets the actor email snapshot.</summary>
    public string? ActorEmail { get; set; }

    /// <summary>Gets or sets the event category.</summary>
    public AuditEventCategory Category { get; set; }

    /// <summary>Gets or sets the specific event type name.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity type.</summary>
    public string? EntityType { get; set; }

    /// <summary>Gets or sets the entity identifier.</summary>
    public string? EntityId { get; set; }

    /// <summary>Gets or sets a human-readable summary.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets optional metadata JSON (no secrets).</summary>
    public string? MetadataJson { get; set; }

    /// <summary>Gets or sets the client IP address.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Gets or sets the correlation identifier.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Gets or sets the UTC timestamp.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Gets or sets a value indicating the row must not be mutated.</summary>
    public bool IsImmutable { get; set; } = true;
}
