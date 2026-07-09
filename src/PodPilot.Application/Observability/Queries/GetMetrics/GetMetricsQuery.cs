using MediatR;
using PodPilot.Contracts.Observability;

namespace PodPilot.Application.Observability.Queries.GetMetrics;

/// <summary>
/// Gets historical metrics snapshots for the current organization.
/// </summary>
public sealed class GetMetricsQuery : IRequest<IReadOnlyList<MetricsSnapshotResponse>>
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
