using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Compliance-related action or status change for an organization.
/// </summary>
public class ComplianceEvent : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the compliance framework.</summary>
    public ComplianceFramework Framework { get; set; }

    /// <summary>Gets or sets the event type (Export, Erasure, RetentionApplied, Assessment).</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the status (Succeeded, Failed, Pending).</summary>
    public string Status { get; set; } = "Succeeded";

    /// <summary>Gets or sets details without PII beyond what is required.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the acting user identifier.</summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>Gets or sets when the event occurred.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;
}
