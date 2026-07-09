using PodPilot.Application.Models.Observability;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Collects and persists metrics snapshots for organizations.
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Collects metrics for an organization and persists a snapshot.
    /// </summary>
    Task<MetricsSnapshotData> CollectAndPersistAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collects usage statistics for an organization and persists a snapshot.
    /// </summary>
    Task<UsageStatisticsData> CollectUsageAndPersistAsync(
        Guid organizationId,
        MetricsPeriodFilter period,
        CancellationToken cancellationToken = default);
}
