using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.ListPodHealthMetrics;

/// <summary>
/// Lists pod health metrics for the current organization.
/// </summary>
public sealed class ListPodHealthMetricsQuery : IRequest<IReadOnlyList<PodHealthMetricResponse>>
{
    /// <summary>Gets or sets an optional pod identifier filter.</summary>
    public Guid? PodId { get; init; }

    /// <summary>Gets or sets the maximum number of records.</summary>
    public int Limit { get; init; } = 100;
}
