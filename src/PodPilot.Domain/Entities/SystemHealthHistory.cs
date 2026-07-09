using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// System health check history.
/// </summary>
public class SystemHealthHistory : Common.BaseEntity
{
    /// <summary>Gets or sets the optional organization identifier.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets when health was recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets or sets the component being monitored.</summary>
    public HealthComponent Component { get; set; }

    /// <summary>Gets or sets the health status.</summary>
    public ObservabilityHealthStatus Status { get; set; }

    /// <summary>Gets or sets the status message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets optional metadata JSON.</summary>
    public string? Metadata { get; set; }

    /// <summary>Gets or sets the optional related entity identifier.</summary>
    public Guid? RelatedEntityId { get; set; }
}
