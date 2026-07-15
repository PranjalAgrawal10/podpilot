namespace PodPilot.Domain.Entities;

/// <summary>
/// Retention and compliance configuration for an organization.
/// </summary>
public class OrganizationComplianceSettings : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets audit/data retention days.</summary>
    public int DataRetentionDays { get; set; } = 365;

    /// <summary>Gets or sets log retention days.</summary>
    public int LogRetentionDays { get; set; } = 90;

    /// <summary>Gets or sets whether GDPR mode controls are enabled.</summary>
    public bool GdprEnabled { get; set; } = true;

    /// <summary>Gets or sets whether SOC 2 control tracking is enabled.</summary>
    public bool Soc2Enabled { get; set; }

    /// <summary>Gets or sets whether ISO 27001 control tracking is enabled.</summary>
    public bool Iso27001Enabled { get; set; }

    /// <summary>Gets or sets the last GDPR export timestamp.</summary>
    public DateTime? LastExportAt { get; set; }

    /// <summary>Gets or sets the last erasure request timestamp.</summary>
    public DateTime? LastErasureAt { get; set; }

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;
}
