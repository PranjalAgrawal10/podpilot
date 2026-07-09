using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Provides usage analytics for organizations.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Gets analytics summary for an organization.
    /// </summary>
    Task<AnalyticsSummary> GetAnalyticsAsync(
        Guid organizationId,
        MetricsPeriod period,
        DateTime? from = null,
        DateTime? to = null,
        Guid? providerId = null,
        Guid? podId = null,
        string? modelName = null,
        CancellationToken cancellationToken = default);
}
