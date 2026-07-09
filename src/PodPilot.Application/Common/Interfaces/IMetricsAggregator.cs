using PodPilot.Application.Models.Observability;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Aggregates live metrics from multiple sources for dashboards.
/// </summary>
public interface IMetricsAggregator
{
    /// <summary>
    /// Gets live dashboard metrics for an organization.
    /// </summary>
    Task<LiveMetricsSnapshot> GetLiveMetricsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
