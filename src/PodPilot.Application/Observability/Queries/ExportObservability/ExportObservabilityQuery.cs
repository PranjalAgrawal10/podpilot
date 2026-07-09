using MediatR;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Observability.Queries.ExportObservability;

/// <summary>
/// Exports observability data for the current organization.
/// </summary>
public sealed class ExportObservabilityQuery : IRequest<ExportResult>
{
    /// <summary>Gets or sets the export format.</summary>
    public ExportFormat Format { get; init; } = ExportFormat.Csv;

    /// <summary>Gets or sets the export type.</summary>
    public ObservabilityExportType ExportType { get; init; } = ObservabilityExportType.Metrics;

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
