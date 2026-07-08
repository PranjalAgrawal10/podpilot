namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Handle for waiting on request completion.
/// </summary>
public sealed class RequestWaitHandle
{
    /// <summary>
    /// Gets or sets the task to await.
    /// </summary>
    public Task<DispatchResult> CompletionTask { get; init; } = null!;

    /// <summary>
    /// Gets or sets the linked cancellation source.
    /// </summary>
    public CancellationTokenSource? LinkedCancellation { get; init; }
}
