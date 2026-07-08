using PodPilot.Application.Common.Interfaces;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Redis-based distributed locking.
/// </summary>
public sealed class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer redis;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDistributedLockService"/> class.
    /// </summary>
    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var token = Guid.NewGuid().ToString("N");
        var acquired = await db.StringSetAsync(
            SchedulerRedisKeys.Lock(lockKey),
            token,
            expiry,
            When.NotExists);

        return acquired ? new RedisLockHandle(db, SchedulerRedisKeys.Lock(lockKey), token) : null;
    }

    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly IDatabase db;
        private readonly string key;
        private readonly string token;

        public RedisLockHandle(IDatabase db, string key, string token)
        {
            this.db = db;
            this.key = key;
            this.token = token;
        }

        public async ValueTask DisposeAsync()
        {
            var value = await db.StringGetAsync(key);
            if (value == token)
            {
                await db.KeyDeleteAsync(key);
            }
        }
    }
}
