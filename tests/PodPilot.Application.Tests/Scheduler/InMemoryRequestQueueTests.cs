using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Scheduler;

namespace PodPilot.Application.Tests.Scheduler;

public class InMemoryRequestQueueTests
{
    [Fact]
    public async Task Enqueue_DequeuesHigherPriorityFirst()
    {
        var queue = new InMemoryRequestQueue();
        var orgId = Guid.NewGuid();

        await queue.EnqueueAsync(new QueuedRequestItem
        {
            RequestId = Guid.NewGuid(),
            OrganizationId = orgId,
            PodId = Guid.NewGuid(),
            Priority = RequestPriority.Low,
            EnqueuedAt = DateTime.UtcNow,
        });

        var highId = Guid.NewGuid();
        await queue.EnqueueAsync(new QueuedRequestItem
        {
            RequestId = highId,
            OrganizationId = orgId,
            PodId = Guid.NewGuid(),
            Priority = RequestPriority.High,
            EnqueuedAt = DateTime.UtcNow,
        });

        var dequeued = await queue.DequeueAsync(orgId);
        Assert.NotNull(dequeued);
        Assert.Equal(highId, dequeued!.RequestId);
    }

    [Fact]
    public async Task IsDuplicate_DetectsRepeatedClientRequestId()
    {
        var queue = new InMemoryRequestQueue();
        var orgId = Guid.NewGuid();
        const string clientId = "req-123";

        Assert.False(await queue.IsDuplicateAsync(orgId, clientId));

        await queue.EnqueueAsync(new QueuedRequestItem
        {
            RequestId = Guid.NewGuid(),
            OrganizationId = orgId,
            PodId = Guid.NewGuid(),
            Priority = RequestPriority.Normal,
            EnqueuedAt = DateTime.UtcNow,
            ClientRequestId = clientId,
        });

        Assert.True(await queue.IsDuplicateAsync(orgId, clientId));
    }
}
