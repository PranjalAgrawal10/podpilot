using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Observability;

/// <summary>
/// Filter for metrics queries.
/// </summary>
public sealed class MetricsFilter
{
    /// <summary>Gets or sets the optional start time.</summary>
    public DateTime? From { get; init; }

    /// <summary>Gets or sets the optional end time.</summary>
    public DateTime? To { get; init; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? PodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets the result limit.</summary>
    public int Limit { get; init; } = 100;
}

/// <summary>
/// Period filter for observability queries.
/// </summary>
public sealed class MetricsPeriodFilter
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; init; } = MetricsPeriod.Hourly;

    /// <summary>Gets or sets the optional start time.</summary>
    public DateTime? From { get; init; }

    /// <summary>Gets or sets the optional end time.</summary>
    public DateTime? To { get; init; }
}

/// <summary>
/// Collected metrics snapshot data.
/// </summary>
public sealed class MetricsSnapshotData
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets when metrics were recorded.</summary>
    public DateTime RecordedAt { get; init; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets CPU utilization percentage.</summary>
    public double CpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets GPU memory used in bytes.</summary>
    public long? GpuMemoryUsedBytes { get; init; }

    /// <summary>Gets or sets RAM used in bytes.</summary>
    public long? MemoryUsedBytes { get; init; }

    /// <summary>Gets or sets disk used in bytes.</summary>
    public long? DiskUsedBytes { get; init; }

    /// <summary>Gets or sets network inbound bytes.</summary>
    public long NetworkInBytes { get; init; }

    /// <summary>Gets or sets network outbound bytes.</summary>
    public long NetworkOutBytes { get; init; }

    /// <summary>Gets or sets active stream count.</summary>
    public int ActiveStreams { get; init; }

    /// <summary>Gets or sets queue size.</summary>
    public int QueueSize { get; init; }

    /// <summary>Gets or sets inference count.</summary>
    public int InferenceCount { get; init; }

    /// <summary>Gets or sets tokens generated.</summary>
    public long TokensGenerated { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; init; }
}

/// <summary>
/// Live dashboard metrics snapshot.
/// </summary>
public sealed class LiveMetricsSnapshot
{
    /// <summary>Gets or sets when metrics were captured.</summary>
    public DateTime CapturedAt { get; init; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets CPU utilization percentage.</summary>
    public double CpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets active stream count.</summary>
    public int ActiveStreams { get; init; }

    /// <summary>Gets or sets queue size.</summary>
    public int QueueSize { get; init; }

    /// <summary>Gets or sets requests per second.</summary>
    public double RequestsPerSecond { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; init; }

    /// <summary>Gets or sets running pod count.</summary>
    public int RunningPods { get; init; }

    /// <summary>Gets or sets healthy pod count.</summary>
    public int HealthyPods { get; init; }

    /// <summary>Gets or sets failed pod count.</summary>
    public int FailedPods { get; init; }

    /// <summary>Gets or sets inference count in the last hour.</summary>
    public int InferenceCountLastHour { get; init; }

    /// <summary>Gets or sets tokens generated in the last hour.</summary>
    public long TokensGeneratedLastHour { get; init; }
}

/// <summary>
/// Cost summary for an organization.
/// </summary>
public sealed class CostSummary
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; init; }

    /// <summary>Gets or sets when the summary was calculated.</summary>
    public DateTime CalculatedAt { get; init; }

    /// <summary>Gets or sets current hourly cost.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets daily cost.</summary>
    public decimal DailyCost { get; init; }

    /// <summary>Gets or sets weekly cost.</summary>
    public decimal WeeklyCost { get; init; }

    /// <summary>Gets or sets monthly cost.</summary>
    public decimal MonthlyCost { get; init; }

    /// <summary>Gets or sets projected monthly cost.</summary>
    public decimal ProjectedMonthlyCost { get; init; }

    /// <summary>Gets or sets savings from auto shutdown.</summary>
    public decimal AutoShutdownSavings { get; init; }

    /// <summary>Gets or sets per-pod cost breakdowns.</summary>
    public IReadOnlyList<PodCostBreakdown> PodBreakdowns { get; init; } = [];

    /// <summary>Gets or sets per-provider cost breakdowns.</summary>
    public IReadOnlyList<ProviderCostBreakdown> ProviderBreakdowns { get; init; } = [];
}

/// <summary>
/// Per-pod cost breakdown.
/// </summary>
public sealed class PodCostBreakdown
{
    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; init; } = string.Empty;

    /// <summary>Gets or sets hourly cost.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets period cost.</summary>
    public decimal PeriodCost { get; init; }
}

/// <summary>
/// Per-provider cost breakdown.
/// </summary>
public sealed class ProviderCostBreakdown
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets hourly cost.</summary>
    public decimal HourlyCost { get; init; }

    /// <summary>Gets or sets period cost.</summary>
    public decimal PeriodCost { get; init; }
}

/// <summary>
/// Usage statistics data.
/// </summary>
public sealed class UsageStatisticsData
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets when statistics were recorded.</summary>
    public DateTime RecordedAt { get; init; }

    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; init; }

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets token count.</summary>
    public long TokenCount { get; init; }

    /// <summary>Gets or sets inference count.</summary>
    public int InferenceCount { get; init; }

    /// <summary>Gets or sets total latency in milliseconds.</summary>
    public long TotalLatencyMs { get; init; }

    /// <summary>Gets or sets error count.</summary>
    public int ErrorCount { get; init; }

    /// <summary>Gets or sets uptime in seconds.</summary>
    public long UptimeSeconds { get; init; }
}

/// <summary>
/// Analytics summary for an organization.
/// </summary>
public sealed class AnalyticsSummary
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; init; }

    /// <summary>Gets or sets total request count.</summary>
    public int TotalRequests { get; init; }

    /// <summary>Gets or sets total token count.</summary>
    public long TotalTokens { get; init; }

    /// <summary>Gets or sets total inference count.</summary>
    public int TotalInferences { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets error rate (0-1).</summary>
    public double ErrorRate { get; init; }

    /// <summary>Gets or sets total uptime in seconds.</summary>
    public long TotalUptimeSeconds { get; init; }

    /// <summary>Gets or sets model usage breakdowns.</summary>
    public IReadOnlyList<ModelUsageBreakdown> ModelBreakdowns { get; init; } = [];

    /// <summary>Gets or sets provider usage breakdowns.</summary>
    public IReadOnlyList<ProviderUsageBreakdown> ProviderBreakdowns { get; init; } = [];

    /// <summary>Gets or sets pod usage breakdowns.</summary>
    public IReadOnlyList<PodUsageBreakdown> PodBreakdowns { get; init; } = [];
}

/// <summary>
/// Model usage breakdown.
/// </summary>
public sealed class ModelUsageBreakdown
{
    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets token count.</summary>
    public long TokenCount { get; init; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }
}

/// <summary>
/// Provider usage breakdown.
/// </summary>
public sealed class ProviderUsageBreakdown
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets inference count.</summary>
    public int InferenceCount { get; init; }
}

/// <summary>
/// Pod usage breakdown.
/// </summary>
public sealed class PodUsageBreakdown
{
    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; init; } = string.Empty;

    /// <summary>Gets or sets request count.</summary>
    public int RequestCount { get; init; }

    /// <summary>Gets or sets uptime in seconds.</summary>
    public long UptimeSeconds { get; init; }
}

/// <summary>
/// System health overview.
/// </summary>
public sealed class SystemHealthOverview
{
    /// <summary>Gets or sets when health was checked.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Gets or sets overall health status.</summary>
    public ObservabilityHealthStatus OverallStatus { get; init; }

    /// <summary>Gets or sets component health entries.</summary>
    public IReadOnlyList<ComponentHealthStatus> Components { get; init; } = [];
}

/// <summary>
/// Health status for a monitored component.
/// </summary>
public sealed class ComponentHealthStatus
{
    /// <summary>Gets or sets the component.</summary>
    public HealthComponent Component { get; init; }

    /// <summary>Gets or sets the health status.</summary>
    public ObservabilityHealthStatus Status { get; init; }

    /// <summary>Gets or sets the status message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional related entity identifier.</summary>
    public Guid? RelatedEntityId { get; init; }
}

/// <summary>
/// Pod health overview.
/// </summary>
public sealed class PodHealthOverview
{
    /// <summary>Gets or sets when health was checked.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Gets or sets total pod count.</summary>
    public int TotalPods { get; init; }

    /// <summary>Gets or sets healthy pod count.</summary>
    public int HealthyPods { get; init; }

    /// <summary>Gets or sets degraded pod count.</summary>
    public int DegradedPods { get; init; }

    /// <summary>Gets or sets unhealthy pod count.</summary>
    public int UnhealthyPods { get; init; }

    /// <summary>Gets or sets per-pod health entries.</summary>
    public IReadOnlyList<PodHealthEntry> Pods { get; init; } = [];
}

/// <summary>
/// Health entry for a single pod.
/// </summary>
public sealed class PodHealthEntry
{
    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; init; } = string.Empty;

    /// <summary>Gets or sets the health status.</summary>
    public ObservabilityHealthStatus Status { get; init; }

    /// <summary>Gets or sets GPU health.</summary>
    public bool GpuHealthy { get; init; }

    /// <summary>Gets or sets Ollama health.</summary>
    public bool OllamaHealthy { get; init; }

    /// <summary>Gets or sets models health.</summary>
    public bool ModelsHealthy { get; init; }

    /// <summary>Gets or sets latency in milliseconds.</summary>
    public int LatencyMs { get; init; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double? GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets the optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime? LastCheckedAt { get; init; }
}

/// <summary>
/// Provider health overview.
/// </summary>
public sealed class ProviderHealthOverview
{
    /// <summary>Gets or sets when health was checked.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Gets or sets total provider count.</summary>
    public int TotalProviders { get; init; }

    /// <summary>Gets or sets healthy provider count.</summary>
    public int HealthyProviders { get; init; }

    /// <summary>Gets or sets unhealthy provider count.</summary>
    public int UnhealthyProviders { get; init; }

    /// <summary>Gets or sets per-provider health entries.</summary>
    public IReadOnlyList<ProviderHealthEntry> Providers { get; init; } = [];
}

/// <summary>
/// Health entry for a single provider.
/// </summary>
public sealed class ProviderHealthEntry
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets the health status.</summary>
    public ObservabilityHealthStatus Status { get; init; }

    /// <summary>Gets or sets response time in milliseconds.</summary>
    public int? ResponseTimeMs { get; init; }

    /// <summary>Gets or sets the optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime? LastCheckedAt { get; init; }
}

/// <summary>
/// Alert summary.
/// </summary>
public sealed class AlertSummary
{
    /// <summary>Gets or sets the alert identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets when the alert was raised.</summary>
    public DateTime RaisedAt { get; init; }

    /// <summary>Gets or sets when the alert was resolved.</summary>
    public DateTime? ResolvedAt { get; init; }

    /// <summary>Gets or sets the alert type.</summary>
    public AlertType AlertType { get; init; }

    /// <summary>Gets or sets the alert severity.</summary>
    public AlertSeverity Severity { get; init; }

    /// <summary>Gets or sets the alert title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets or sets the alert message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets a value indicating whether the alert is active.</summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Export type for observability data.
/// </summary>
public enum ObservabilityExportType
{
    /// <summary>Export metrics snapshots.</summary>
    Metrics = 0,

    /// <summary>Export cost snapshots.</summary>
    Cost = 1,

    /// <summary>Export usage statistics.</summary>
    Usage = 2,

    /// <summary>Export alert history.</summary>
    Alerts = 3,

    /// <summary>Export system health history.</summary>
    Health = 4,
}

/// <summary>
/// Filter for observability export.
/// </summary>
public sealed class ObservabilityExportFilter
{
    /// <summary>Gets or sets the optional start time.</summary>
    public DateTime? From { get; init; }

    /// <summary>Gets or sets the optional end time.</summary>
    public DateTime? To { get; init; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? PodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }
}

/// <summary>
/// Result of an observability export operation.
/// </summary>
public sealed class ExportResult
{
    /// <summary>Gets or sets the file content.</summary>
    public byte[] Content { get; init; } = [];

    /// <summary>Gets or sets the content type.</summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Gets or sets the file name.</summary>
    public string FileName { get; init; } = string.Empty;
}
