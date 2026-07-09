using PodPilot.Application.Models.Observability;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Observability;

/// <summary>
/// Maps observability entities and models to contract responses.
/// </summary>
internal static class ObservabilityMapper
{
    /// <summary>
    /// Maps a metrics snapshot entity to a response DTO.
    /// </summary>
    public static MetricsSnapshotResponse ToMetricsSnapshotResponse(MetricsSnapshot snapshot) =>
        new()
        {
            Id = snapshot.Id,
            RecordedAt = snapshot.RecordedAt,
            ProviderId = snapshot.ProviderId,
            GpuPodId = snapshot.GpuPodId,
            ModelName = snapshot.ModelName,
            GpuUtilizationPercent = snapshot.GpuUtilizationPercent,
            GpuMemoryUsedBytes = snapshot.GpuMemoryUsedBytes,
            GpuMemoryTotalBytes = snapshot.GpuMemoryTotalBytes,
            CpuUtilizationPercent = snapshot.CpuUtilizationPercent,
            MemoryUsedBytes = snapshot.MemoryUsedBytes,
            MemoryTotalBytes = snapshot.MemoryTotalBytes,
            DiskUsedBytes = snapshot.DiskUsedBytes,
            DiskTotalBytes = snapshot.DiskTotalBytes,
            NetworkInBytes = snapshot.NetworkInBytes,
            NetworkOutBytes = snapshot.NetworkOutBytes,
            TemperatureCelsius = snapshot.TemperatureCelsius,
            PowerWatts = snapshot.PowerWatts,
            ActiveStreams = snapshot.ActiveStreams,
            QueueSize = snapshot.QueueSize,
            InferenceCount = snapshot.InferenceCount,
            TokensGenerated = snapshot.TokensGenerated,
            AverageLatencyMs = snapshot.AverageLatencyMs,
            ErrorRate = snapshot.ErrorRate,
        };

    /// <summary>
    /// Maps live metrics to a response DTO.
    /// </summary>
    public static LiveMetricsResponse ToLiveMetricsResponse(LiveMetricsSnapshot snapshot) =>
        new()
        {
            CapturedAt = snapshot.CapturedAt,
            GpuUtilizationPercent = snapshot.GpuUtilizationPercent,
            CpuUtilizationPercent = snapshot.CpuUtilizationPercent,
            ActiveStreams = snapshot.ActiveStreams,
            QueueSize = snapshot.QueueSize,
            RequestsPerSecond = snapshot.RequestsPerSecond,
            AverageLatencyMs = snapshot.AverageLatencyMs,
            ErrorRate = snapshot.ErrorRate,
            RunningPods = snapshot.RunningPods,
            HealthyPods = snapshot.HealthyPods,
            FailedPods = snapshot.FailedPods,
            InferenceCountLastHour = snapshot.InferenceCountLastHour,
            TokensGeneratedLastHour = snapshot.TokensGeneratedLastHour,
        };

    /// <summary>
    /// Maps cost summary to a response DTO.
    /// </summary>
    public static CostResponse ToCostResponse(CostSummary summary) =>
        new()
        {
            Period = summary.Period.ToString(),
            CalculatedAt = summary.CalculatedAt,
            HourlyCost = summary.HourlyCost,
            DailyCost = summary.DailyCost,
            WeeklyCost = summary.WeeklyCost,
            MonthlyCost = summary.MonthlyCost,
            ProjectedMonthlyCost = summary.ProjectedMonthlyCost,
            AutoShutdownSavings = summary.AutoShutdownSavings,
            PodBreakdowns = summary.PodBreakdowns.Select(b => new PodCostBreakdownResponse
            {
                PodId = b.PodId,
                PodName = b.PodName,
                HourlyCost = b.HourlyCost,
                PeriodCost = b.PeriodCost,
            }).ToList(),
            ProviderBreakdowns = summary.ProviderBreakdowns.Select(b => new ProviderCostBreakdownResponse
            {
                ProviderId = b.ProviderId,
                ProviderName = b.ProviderName,
                HourlyCost = b.HourlyCost,
                PeriodCost = b.PeriodCost,
            }).ToList(),
        };

    /// <summary>
    /// Maps analytics summary to a response DTO.
    /// </summary>
    public static AnalyticsResponse ToAnalyticsResponse(AnalyticsSummary summary) =>
        new()
        {
            Period = summary.Period.ToString(),
            TotalRequests = summary.TotalRequests,
            TotalTokens = summary.TotalTokens,
            TotalInferences = summary.TotalInferences,
            AverageLatencyMs = summary.AverageLatencyMs,
            ErrorRate = summary.ErrorRate,
            TotalUptimeSeconds = summary.TotalUptimeSeconds,
            ModelBreakdowns = summary.ModelBreakdowns.Select(b => new ModelUsageBreakdownResponse
            {
                ModelName = b.ModelName,
                RequestCount = b.RequestCount,
                TokenCount = b.TokenCount,
                AverageLatencyMs = b.AverageLatencyMs,
            }).ToList(),
            ProviderBreakdowns = summary.ProviderBreakdowns.Select(b => new ProviderUsageBreakdownResponse
            {
                ProviderId = b.ProviderId,
                ProviderName = b.ProviderName,
                RequestCount = b.RequestCount,
                InferenceCount = b.InferenceCount,
            }).ToList(),
            PodBreakdowns = summary.PodBreakdowns.Select(b => new PodUsageBreakdownResponse
            {
                PodId = b.PodId,
                PodName = b.PodName,
                RequestCount = b.RequestCount,
                UptimeSeconds = b.UptimeSeconds,
            }).ToList(),
        };

    /// <summary>
    /// Maps system health overview to a response DTO.
    /// </summary>
    public static SystemHealthResponse ToSystemHealthResponse(SystemHealthOverview overview) =>
        new()
        {
            CheckedAt = overview.CheckedAt,
            OverallStatus = overview.OverallStatus.ToString(),
            Components = overview.Components.Select(c => new ComponentHealthResponse
            {
                Component = c.Component.ToString(),
                Status = c.Status.ToString(),
                Message = c.Message,
                RelatedEntityId = c.RelatedEntityId,
            }).ToList(),
        };

    /// <summary>
    /// Maps pod health overview to a response DTO.
    /// </summary>
    public static PodHealthOverviewResponse ToPodHealthOverviewResponse(PodHealthOverview overview) =>
        new()
        {
            CheckedAt = overview.CheckedAt,
            TotalPods = overview.TotalPods,
            HealthyPods = overview.HealthyPods,
            DegradedPods = overview.DegradedPods,
            UnhealthyPods = overview.UnhealthyPods,
            Pods = overview.Pods.Select(p => new PodHealthEntryResponse
            {
                PodId = p.PodId,
                PodName = p.PodName,
                Status = p.Status.ToString(),
                GpuHealthy = p.GpuHealthy,
                OllamaHealthy = p.OllamaHealthy,
                ModelsHealthy = p.ModelsHealthy,
                LatencyMs = p.LatencyMs,
                GpuUtilizationPercent = p.GpuUtilizationPercent,
                ErrorMessage = p.ErrorMessage,
                LastCheckedAt = p.LastCheckedAt,
            }).ToList(),
        };

    /// <summary>
    /// Maps provider health overview to a response DTO.
    /// </summary>
    public static ProviderHealthOverviewResponse ToProviderHealthOverviewResponse(ProviderHealthOverview overview) =>
        new()
        {
            CheckedAt = overview.CheckedAt,
            TotalProviders = overview.TotalProviders,
            HealthyProviders = overview.HealthyProviders,
            UnhealthyProviders = overview.UnhealthyProviders,
            Providers = overview.Providers.Select(p => new ProviderHealthEntryResponse
            {
                ProviderId = p.ProviderId,
                ProviderName = p.ProviderName,
                Status = p.Status.ToString(),
                ResponseTimeMs = p.ResponseTimeMs,
                ErrorMessage = p.ErrorMessage,
                LastCheckedAt = p.LastCheckedAt,
            }).ToList(),
        };

    /// <summary>
    /// Maps alert history entity to a response DTO.
    /// </summary>
    public static AlertResponse ToAlertResponse(AlertHistory alert) =>
        new()
        {
            Id = alert.Id,
            RaisedAt = alert.RaisedAt,
            ResolvedAt = alert.ResolvedAt,
            AlertType = alert.AlertType.ToString(),
            Severity = alert.Severity.ToString(),
            Title = alert.Title,
            Message = alert.Message,
            ProviderId = alert.ProviderId,
            GpuPodId = alert.GpuPodId,
            ModelName = alert.ModelName,
            IsActive = alert.IsActive,
        };

    /// <summary>
    /// Maps alert summary to a response DTO.
    /// </summary>
    public static AlertResponse ToAlertResponse(AlertSummary alert) =>
        new()
        {
            Id = alert.Id,
            RaisedAt = alert.RaisedAt,
            ResolvedAt = alert.ResolvedAt,
            AlertType = alert.AlertType.ToString(),
            Severity = alert.Severity.ToString(),
            Title = alert.Title,
            Message = alert.Message,
            ProviderId = alert.ProviderId,
            GpuPodId = alert.GpuPodId,
            ModelName = alert.ModelName,
            IsActive = alert.IsActive,
        };

    /// <summary>
    /// Parses a metrics period from string.
    /// </summary>
    public static MetricsPeriod ParseMetricsPeriod(string? value) =>
        Enum.TryParse<MetricsPeriod>(value, ignoreCase: true, out var period)
            ? period
            : MetricsPeriod.Hourly;

    /// <summary>
    /// Parses an export format from string.
    /// </summary>
    public static ExportFormat ParseExportFormat(string? value) =>
        Enum.TryParse<ExportFormat>(value, ignoreCase: true, out var format)
            ? format
            : ExportFormat.Csv;

    /// <summary>
    /// Parses an export type from string.
    /// </summary>
    public static ObservabilityExportType ParseExportType(string? value) =>
        Enum.TryParse<ObservabilityExportType>(value, ignoreCase: true, out var exportType)
            ? exportType
            : ObservabilityExportType.Metrics;
}
