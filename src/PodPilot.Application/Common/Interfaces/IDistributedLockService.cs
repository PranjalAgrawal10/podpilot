namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Distributed locking for scheduler coordination.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Attempts to acquire a lock.
    /// </summary>
    Task<IAsyncDisposable?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}
