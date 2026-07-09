using PodPilot.Application.Models.Observability;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Monitors system health and manages alerts.
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// Runs health checks and persists results for an organization.
    /// </summary>
    Task<SystemHealthOverview> RunHealthChecksAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pod health overview for an organization.
    /// </summary>
    Task<PodHealthOverview> GetPodHealthOverviewAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets provider health overview for an organization.
    /// </summary>
    Task<ProviderHealthOverview> GetProviderHealthOverviewAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates alert conditions and persists alert history.
    /// </summary>
    Task<IReadOnlyList<AlertSummary>> EvaluateAlertsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
