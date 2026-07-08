using System.Collections.Concurrent;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Scheduler;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Tracks waiting HTTP connections for queued requests.
/// </summary>
public sealed class RequestTracker : IRequestTracker
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<DispatchResult>> waiters = new();

    /// <inheritdoc />
    public int ActiveCount => waiters.Count;

    /// <inheritdoc />
    public Task<RequestWaitHandle> RegisterWaitAsync(
        Guid requestId,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DispatchResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        waiters[requestId] = tcs;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        cts.Token.Register(() =>
        {
            if (tcs.TrySetCanceled())
            {
                waiters.TryRemove(requestId, out _);
            }
        });

        return Task.FromResult(new RequestWaitHandle
        {
            CompletionTask = tcs.Task,
            LinkedCancellation = cts,
        });
    }

    /// <inheritdoc />
    public void Complete(Guid requestId, DispatchResult result)
    {
        if (waiters.TryRemove(requestId, out var tcs))
        {
            tcs.TrySetResult(result);
        }
    }

    /// <inheritdoc />
    public void Fail(Guid requestId, string errorMessage, int statusCode = 502)
    {
        if (waiters.TryRemove(requestId, out var tcs))
        {
            tcs.TrySetResult(new DispatchResult
            {
                Success = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
            });
        }
    }

    /// <inheritdoc />
    public void Cancel(Guid requestId)
    {
        if (waiters.TryRemove(requestId, out var tcs))
        {
            tcs.TrySetCanceled();
        }
    }
}
