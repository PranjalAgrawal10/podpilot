using System.Collections.Concurrent;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// In-memory priority queue used in testing and when Redis is unavailable.
/// </summary>
public sealed class InMemoryRequestQueue : IRequestQueue
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<RequestPriority, ConcurrentQueue<QueuedRequestItem>>> queues = new();
    private readonly ConcurrentDictionary<string, byte> duplicates = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<EnqueueResult> EnqueueAsync(QueuedRequestItem item, CancellationToken cancellationToken = default)
    {
        var orgQueue = queues.GetOrAdd(item.OrganizationId, _ => new ConcurrentDictionary<RequestPriority, ConcurrentQueue<QueuedRequestItem>>());
        var priorityQueue = orgQueue.GetOrAdd(item.Priority, _ => new ConcurrentQueue<QueuedRequestItem>());
        priorityQueue.Enqueue(item);

        if (!string.IsNullOrWhiteSpace(item.ClientRequestId))
        {
            duplicates.TryAdd($"{item.OrganizationId}:{item.ClientRequestId}", 0);
        }

        var position = GetPosition(item.OrganizationId);
        return Task.FromResult(new EnqueueResult { Success = true, Position = position });
    }

    /// <inheritdoc />
    public Task<QueuedRequestItem?> DequeueAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        if (!queues.TryGetValue(organizationId, out var orgQueue))
        {
            return Task.FromResult<QueuedRequestItem?>(null);
        }

        foreach (var priority in Enum.GetValues<RequestPriority>().OrderByDescending(p => p))
        {
            if (orgQueue.TryGetValue(priority, out var queue) && queue.TryDequeue(out var item))
            {
                return Task.FromResult<QueuedRequestItem?>(item);
            }
        }

        return Task.FromResult<QueuedRequestItem?>(null);
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(Guid requestId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        if (!queues.TryGetValue(organizationId, out var orgQueue))
        {
            return Task.FromResult(false);
        }

        var removed = false;
        foreach (var queue in orgQueue.Values)
        {
            var remaining = new List<QueuedRequestItem>();
            while (queue.TryDequeue(out var item))
            {
                if (item.RequestId == requestId)
                {
                    removed = true;
                    continue;
                }

                remaining.Add(item);
            }

            foreach (var item in remaining)
            {
                queue.Enqueue(item);
            }
        }

        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public Task<int> GetLengthAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        if (!queues.TryGetValue(organizationId, out var orgQueue))
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(orgQueue.Values.Sum(q => q.Count));
    }

    /// <inheritdoc />
    public Task<bool> IsDuplicateAsync(Guid organizationId, string clientRequestId, CancellationToken cancellationToken = default)
    {
        var key = $"{organizationId}:{clientRequestId}";
        return Task.FromResult(duplicates.ContainsKey(key));
    }

    private int GetPosition(Guid organizationId)
    {
        if (!queues.TryGetValue(organizationId, out var orgQueue))
        {
            return 1;
        }

        return orgQueue.Values.Sum(q => q.Count);
    }
}
