using MediatR;
using PodPilot.Contracts.Scheduler;

namespace PodPilot.Application.Scheduler.Queries.GetSchedulerStatus;

/// <summary>
/// Gets global scheduler status for the current organization.
/// </summary>
public sealed class GetSchedulerStatusQuery : IRequest<SchedulerStatusResponse>
{
}
