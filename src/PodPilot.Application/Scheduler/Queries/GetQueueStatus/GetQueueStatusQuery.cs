using MediatR;
using PodPilot.Contracts.Scheduler;

namespace PodPilot.Application.Scheduler.Queries.GetQueueStatus;

/// <summary>
/// Gets queue metrics for the current organization.
/// </summary>
public sealed class GetQueueStatusQuery : IRequest<QueueStatusResponse>
{
}
