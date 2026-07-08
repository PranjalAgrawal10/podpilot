using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Selects Redis or in-memory scheduler backends based on availability.
/// </summary>
internal sealed class SchedulerInfrastructure
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerInfrastructure"/> class.
    /// </summary>
    public SchedulerInfrastructure(
        IRequestQueue queue,
        IDistributedLockService lockService,
        IConnectionMultiplexer? redis)
    {
        Queue = queue;
        LockService = lockService;
        Redis = redis;
    }

    /// <summary>
    /// Gets the request queue implementation.
    /// </summary>
    public IRequestQueue Queue { get; }

    /// <summary>
    /// Gets the distributed lock implementation.
    /// </summary>
    public IDistributedLockService LockService { get; }

    /// <summary>
    /// Gets the Redis connection when Redis is in use.
    /// </summary>
    public IConnectionMultiplexer? Redis { get; }

    /// <summary>
    /// Creates scheduler infrastructure, preferring Redis when reachable.
    /// </summary>
    public static SchedulerInfrastructure Create(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger logger)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return new SchedulerInfrastructure(
                new InMemoryRequestQueue(),
                new InMemoryDistributedLockService(),
                redis: null);
        }

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection) &&
            TryConnectRedis(redisConnection, out var redis))
        {
            logger.LogInformation("Scheduler using Redis at {RedisEndpoint}", redisConnection);
            return new SchedulerInfrastructure(
                new RedisRequestQueue(redis),
                new RedisDistributedLockService(redis),
                redis);
        }

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            logger.LogWarning(
                "Redis is configured at {RedisEndpoint} but is unreachable. Falling back to in-memory scheduler queue.",
                redisConnection);
        }
        else
        {
            logger.LogInformation("Redis is not configured. Using in-memory scheduler queue.");
        }

        return new SchedulerInfrastructure(
            new InMemoryRequestQueue(),
            new InMemoryDistributedLockService(),
            redis: null);
    }

    private static bool TryConnectRedis(string connectionString, out IConnectionMultiplexer redis)
    {
        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = true;
            options.ConnectTimeout = 2000;
            options.SyncTimeout = 2000;

            redis = ConnectionMultiplexer.Connect(options);
            return redis.IsConnected;
        }
        catch
        {
            redis = null!;
            return false;
        }
    }
}
