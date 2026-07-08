using System.Text.Json;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Redis-backed priority queue for scheduled requests.
/// </summary>
public sealed class RedisRequestQueue : IRequestQueue
{
    private readonly IConnectionMultiplexer redis;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisRequestQueue"/> class.
    /// </summary>
    public RedisRequestQueue(IConnectionMultiplexer redis)
    {
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<EnqueueResult> EnqueueAsync(QueuedRequestItem item, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var length = await db.SortedSetLengthAsync(SchedulerRedisKeys.Queue(item.OrganizationId));
        if (length >= ApplicationConstants.SchedulerMaxQueueLength)
        {
            return new EnqueueResult
            {
                Success = false,
                ErrorMessage = "Scheduler queue is full.",
            };
        }

        var score = BuildScore(item.Priority, item.EnqueuedAt);
        var payload = JsonSerializer.Serialize(item);
        await db.SortedSetAddAsync(SchedulerRedisKeys.Queue(item.OrganizationId), payload, score);

        if (!string.IsNullOrWhiteSpace(item.ClientRequestId))
        {
            await db.StringSetAsync(
                SchedulerRedisKeys.Duplicate(item.OrganizationId, item.ClientRequestId),
                item.RequestId.ToString(),
                TimeSpan.FromHours(24),
                When.NotExists);
        }

        var position = (int)await db.SortedSetLengthAsync(SchedulerRedisKeys.Queue(item.OrganizationId));
        return new EnqueueResult { Success = true, Position = position };
    }

    /// <inheritdoc />
    public async Task<QueuedRequestItem?> DequeueAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var entries = await db.SortedSetRangeByRankAsync(SchedulerRedisKeys.Queue(organizationId), 0, 0, Order.Descending);
        if (entries.Length == 0)
        {
            return null;
        }

        var payload = entries[0].ToString();
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        await db.SortedSetRemoveAsync(SchedulerRedisKeys.Queue(organizationId), entries[0]);
        return JsonSerializer.Deserialize<QueuedRequestItem>(payload);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(Guid requestId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var entries = await db.SortedSetRangeByRankAsync(SchedulerRedisKeys.Queue(organizationId), 0, -1, Order.Descending);
        foreach (var entry in entries)
        {
            var item = JsonSerializer.Deserialize<QueuedRequestItem>(entry.ToString()!);
            if (item?.RequestId == requestId)
            {
                await db.SortedSetRemoveAsync(SchedulerRedisKeys.Queue(organizationId), entry);
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<int> GetLengthAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        return (int)await db.SortedSetLengthAsync(SchedulerRedisKeys.Queue(organizationId));
    }

    /// <inheritdoc />
    public async Task<bool> IsDuplicateAsync(Guid organizationId, string clientRequestId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        return await db.KeyExistsAsync(SchedulerRedisKeys.Duplicate(organizationId, clientRequestId));
    }

    private static double BuildScore(RequestPriority priority, DateTime enqueuedAt)
    {
        var priorityWeight = (int)priority * 1_000_000_000_000L;
        var timeComponent = DateTime.UtcNow.Ticks / 1_000_000.0;
        return priorityWeight + timeComponent;
    }
}
