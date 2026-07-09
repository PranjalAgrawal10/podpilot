using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Usage statistics for analytics.
/// </summary>
public class UsageStatistics : Common.BaseEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets when statistics were recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; set; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; set; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; set; }

    /// <summary>Gets or sets token count.</summary>
    public long TokenCount { get; set; }

    /// <summary>Gets or sets inference count.</summary>
    public int InferenceCount { get; set; }

    /// <summary>Gets or sets total latency in milliseconds.</summary>
    public long TotalLatencyMs { get; set; }

    /// <summary>Gets or sets error count.</summary>
    public int ErrorCount { get; set; }

    /// <summary>Gets or sets uptime in seconds.</summary>
    public long UptimeSeconds { get; set; }
}
