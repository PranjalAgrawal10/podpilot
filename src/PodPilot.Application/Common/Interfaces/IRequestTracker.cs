using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Scheduler;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Tracks in-flight request completion for waiting HTTP connections.
/// </summary>
public interface IRequestTracker
{
    /// <summary>
    /// Gets the number of active tracked requests.
    /// </summary>
    int ActiveCount { get; }

    /// <summary>
    /// Registers a waiter for a request completion.
    /// </summary>
    Task<RequestWaitHandle> RegisterWaitAsync(
        Guid requestId,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a waiting request with a successful result.
    /// </summary>
    void Complete(Guid requestId, DispatchResult result);

    /// <summary>
    /// Fails a waiting request.
    /// </summary>
    void Fail(Guid requestId, string errorMessage, int statusCode = 502);

    /// <summary>
    /// Cancels a waiting request.
    /// </summary>
    void Cancel(Guid requestId);
}
