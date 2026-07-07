using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an immutable audit log entry.
/// </summary>
public class AuditLog : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the user who performed the action.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the type of entity affected.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the affected entity.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action performed.
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// Gets or sets optional details about the action.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the client IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the request correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the action.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
