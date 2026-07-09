using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Alert history record.
/// </summary>
public class AlertHistory : Common.BaseEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets when the alert was raised.</summary>
    public DateTime RaisedAt { get; set; }

    /// <summary>Gets or sets when the alert was resolved.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Gets or sets the alert type.</summary>
    public AlertType AlertType { get; set; }

    /// <summary>Gets or sets the alert severity.</summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>Gets or sets the alert title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the alert message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; set; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets a value indicating whether the alert is active.</summary>
    public bool IsActive { get; set; } = true;
}
