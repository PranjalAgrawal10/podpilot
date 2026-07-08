using System.Collections.Concurrent;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// In-memory distributed lock for testing.
/// </summary>
public sealed class InMemoryDistributedLockService : IDistributedLockService
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var semaphore = locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        var acquired = await semaphore.WaitAsync(0, cancellationToken);
        return acquired ? new LockHandle(semaphore, expiry) : null;
    }

    private sealed class LockHandle : IAsyncDisposable
    {
        private readonly SemaphoreSlim semaphore;
        private readonly CancellationTokenSource cts;
        private int disposed;

        public LockHandle(SemaphoreSlim semaphore, TimeSpan expiry)
        {
            this.semaphore = semaphore;
            cts = new CancellationTokenSource(expiry);
            cts.Token.Register(() =>
            {
                if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
                {
                    semaphore.Release();
                }
            });
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
            {
                cts.Cancel();
                semaphore.Release();
            }

            return ValueTask.CompletedTask;
        }
    }
}
