using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Exports observability data in various formats.
/// </summary>
public interface IObservabilityExportService
{
    /// <summary>
    /// Exports observability data for an organization.
    /// </summary>
    Task<ExportResult> ExportAsync(
        Guid organizationId,
        ExportFormat format,
        ObservabilityExportType exportType,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken = default);
}
