using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Priority queue storage for scheduled requests.
/// </summary>
public interface IRequestQueue
{
    /// <summary>
    /// Enqueues a request.
    /// </summary>
    Task<EnqueueResult> EnqueueAsync(
        QueuedRequestItem item,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the next eligible request for an organization.
    /// </summary>
    Task<QueuedRequestItem?> DequeueAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a request from the queue.
    /// </summary>
    Task<bool> RemoveAsync(
        Guid requestId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the queue length for an organization.
    /// </summary>
    Task<int> GetLengthAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a duplicate client request identifier exists.
    /// </summary>
    Task<bool> IsDuplicateAsync(
        Guid organizationId,
        string clientRequestId,
        CancellationToken cancellationToken = default);
}
