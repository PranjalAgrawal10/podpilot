using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates intelligent request scheduling for the AI gateway.
/// </summary>
public interface IRequestScheduler
{
    /// <summary>
    /// Schedules and processes a gateway request through the scheduling pipeline.
    /// </summary>
    Task<ScheduledRequestResult> ProcessAsync(
        ScheduleRequestContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a queued or running request.
    /// </summary>
    Task<bool> CancelAsync(
        Guid requestId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed request.
    /// </summary>
    Task<bool> RetryAsync(
        Guid requestId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as completed.
    /// </summary>
    Task CompleteAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as failed.
    /// </summary>
    Task FailAsync(
        Guid requestId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as timed out.
    /// </summary>
    Task TimeoutAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reassigns a request to a different pod.
    /// </summary>
    Task<bool> ReassignAsync(
        Guid requestId,
        Guid newPodId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a dequeued request.
    /// </summary>
    Task ProcessQueuedItemAsync(
        QueuedRequestItem item,
        CancellationToken cancellationToken = default);
}
