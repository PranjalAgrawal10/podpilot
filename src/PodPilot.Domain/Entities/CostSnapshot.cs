using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Point-in-time cost snapshot for an organization.
/// </summary>
public class CostSnapshot : Common.BaseEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets when the snapshot was recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; set; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; set; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets current hourly cost.</summary>
    public decimal HourlyCost { get; set; }

    /// <summary>Gets or sets daily cost.</summary>
    public decimal DailyCost { get; set; }

    /// <summary>Gets or sets weekly cost.</summary>
    public decimal WeeklyCost { get; set; }

    /// <summary>Gets or sets monthly cost.</summary>
    public decimal MonthlyCost { get; set; }

    /// <summary>Gets or sets projected monthly cost.</summary>
    public decimal ProjectedMonthlyCost { get; set; }

    /// <summary>Gets or sets savings from auto shutdown.</summary>
    public decimal AutoShutdownSavings { get; set; }
}
