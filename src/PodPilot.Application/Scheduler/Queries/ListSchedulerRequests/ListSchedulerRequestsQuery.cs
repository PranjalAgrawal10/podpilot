using MediatR;
using PodPilot.Contracts.Scheduler;

namespace PodPilot.Application.Scheduler.Queries.ListSchedulerRequests;

/// <summary>
/// Lists scheduler requests for the current organization.
/// </summary>
public sealed class ListSchedulerRequestsQuery : IRequest<IReadOnlyList<SchedulerRequestResponse>>
{
    /// <summary>
    /// Gets or sets an optional status filter.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of results.
    /// </summary>
    public int Limit { get; init; } = 50;
}
