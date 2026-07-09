using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Calculates cost snapshots for organizations.
/// </summary>
public interface ICostCalculator
{
    /// <summary>
    /// Calculates cost summary for an organization.
    /// </summary>
    Task<CostSummary> CalculateAsync(
        Guid organizationId,
        MetricsPeriod period,
        Guid? providerId = null,
        Guid? podId = null,
        string? modelName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates and persists a cost snapshot for an organization.
    /// </summary>
    Task<CostSummary> CalculateAndPersistAsync(
        Guid organizationId,
        MetricsPeriod period,
        CancellationToken cancellationToken = default);
}
