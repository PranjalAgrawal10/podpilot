using MediatR;
using PodPilot.Contracts.Scheduler;

namespace PodPilot.Application.Scheduler.Queries.GetSchedulerRequest;

/// <summary>
/// Gets a scheduler request by identifier.
/// </summary>
public sealed class GetSchedulerRequestQuery : IRequest<SchedulerRequestResponse>
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public Guid RequestId { get; init; }
}
