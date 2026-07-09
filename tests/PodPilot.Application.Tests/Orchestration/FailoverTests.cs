using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Lifecycle;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Orchestrator;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Scheduler;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Orchestration;

public class FailoverTests
{
    [Fact]
    public async Task Should_Reassign_Queued_Requests_To_Replacement_Pod()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var failedPodId = Guid.NewGuid();
        var replacementPodId = Guid.NewGuid();
        var queuedRequestId = Guid.NewGuid();
        var activeRequestId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await SeedFailoverScenarioAsync(
            dbContext,
            organizationId,
            poolId,
            failedPodId,
            replacementPodId,
            queuedRequestId,
            activeRequestId,
            now);

        var lifecycleService = new Mock<IPodLifecycleService>();
        lifecycleService.Setup(s => s.WakePodAsync(
                replacementPodId,
                organizationId,
                "orchestrator-failover",
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PodWakeResult { Success = true, Status = "Running" });

        var orchestrator = CreateOrchestrator(dbContext, lifecycleService.Object, now);
        var result = await orchestrator.HandleFailoverAsync(
            organizationId,
            failedPodId,
            activeRequestId);

        Assert.True(result.Success);
        Assert.Equal(replacementPodId, result.ReplacementPodId);
        Assert.Equal(2, result.ReassignedRequestCount);

        var failedMember = await dbContext.PodPoolMembers
            .FirstAsync(m => m.GpuPodId == failedPodId);
        Assert.Equal(OrchestrationPodState.Failed, failedMember.State);

        var reassignedRequests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId && r.GpuPodId == replacementPodId)
            .Select(r => r.Id)
            .ToListAsync();

        Assert.Contains(queuedRequestId, reassignedRequests);
        Assert.Contains(activeRequestId, reassignedRequests);

        var reassignmentEvents = await dbContext.SchedulerEvents
            .Where(e => e.OrganizationId == organizationId && e.EventType == SchedulerEventType.Reassigned)
            .CountAsync();
        Assert.Equal(2, reassignmentEvents);
    }

    [Fact]
    public async Task Should_Return_Failure_When_No_Replacement_Pod_Is_Available()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var failedPodId = Guid.NewGuid();
        var queuedRequestId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await SeedSinglePodPoolAsync(
            dbContext,
            organizationId,
            poolId,
            failedPodId,
            queuedRequestId,
            now);

        var lifecycleService = new Mock<IPodLifecycleService>();
        var orchestrator = CreateOrchestrator(dbContext, lifecycleService.Object, now);
        var result = await orchestrator.HandleFailoverAsync(
            organizationId,
            failedPodId,
            null);

        Assert.False(result.Success);
        Assert.Null(result.ReplacementPodId);
        Assert.Equal(0, result.ReassignedRequestCount);
        Assert.Equal("No replacement pod was available.", result.ErrorMessage);

        lifecycleService.Verify(
            s => s.WakePodAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static PodOrchestratorService CreateOrchestrator(
        ApplicationDbContext dbContext,
        IPodLifecycleService lifecycleService,
        DateTime utcNow)
    {
        var podPoolManager = new PodPoolManager(
            dbContext,
            new NoOpOrchestratorNotificationService(),
            new FixedDateTimeService(utcNow),
            NullLogger<PodPoolManager>.Instance);

        var loadBalancer = new Mock<ILoadBalancer>();
        var requestQueue = new InMemoryRequestQueue();

        return new PodOrchestratorService(
            dbContext,
            podPoolManager,
            loadBalancer.Object,
            requestQueue,
            lifecycleService,
            new NoOpOrchestratorNotificationService(),
            new FixedDateTimeService(utcNow),
            NullLogger<PodOrchestratorService>.Instance);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"failover-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedFailoverScenarioAsync(
        ApplicationDbContext dbContext,
        Guid organizationId,
        Guid poolId,
        Guid failedPodId,
        Guid replacementPodId,
        Guid queuedRequestId,
        Guid activeRequestId,
        DateTime now)
    {
        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = poolId,
                OrganizationId = organizationId,
                Name = "failover-pool",
                IsActive = true,
                CreatedAt = now,
            });

        await SeedPodMemberAsync(dbContext, poolId, failedPodId, organizationId, OrchestrationPodState.Healthy, now);
        await SeedPodMemberAsync(dbContext, poolId, replacementPodId, organizationId, OrchestrationPodState.Healthy, now);

        await dbContext.AddGatewayRequestAsync(
            new GatewayRequest
            {
                Id = queuedRequestId,
                OrganizationId = organizationId,
                GpuPodId = failedPodId,
                Status = GatewayRequestStatus.Queued,
                HttpMethod = "POST",
                Path = "/v1/chat/completions",
                CreatedAt = now,
                StartedAt = now,
            });

        await dbContext.AddGatewayRequestAsync(
            new GatewayRequest
            {
                Id = activeRequestId,
                OrganizationId = organizationId,
                GpuPodId = failedPodId,
                Status = GatewayRequestStatus.Streaming,
                HttpMethod = "POST",
                Path = "/v1/chat/completions",
                CreatedAt = now,
                StartedAt = now,
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSinglePodPoolAsync(
        ApplicationDbContext dbContext,
        Guid organizationId,
        Guid poolId,
        Guid failedPodId,
        Guid queuedRequestId,
        DateTime now)
    {
        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = poolId,
                OrganizationId = organizationId,
                Name = "single-pod-pool",
                IsActive = true,
                CreatedAt = now,
            });

        await SeedPodMemberAsync(dbContext, poolId, failedPodId, organizationId, OrchestrationPodState.Healthy, now);

        await dbContext.AddGatewayRequestAsync(
            new GatewayRequest
            {
                Id = queuedRequestId,
                OrganizationId = organizationId,
                GpuPodId = failedPodId,
                Status = GatewayRequestStatus.Queued,
                HttpMethod = "POST",
                Path = "/v1/chat/completions",
                CreatedAt = now,
                StartedAt = now,
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPodMemberAsync(
        ApplicationDbContext dbContext,
        Guid poolId,
        Guid podId,
        Guid organizationId,
        OrchestrationPodState state,
        DateTime now)
    {
        await dbContext.AddGpuPodAsync(
            new GpuPod
            {
                Id = podId,
                OrganizationId = organizationId,
                ProviderId = Guid.NewGuid(),
                Name = $"pod-{podId:N}"[..12],
                Endpoint = "http://127.0.0.1:11434",
                Status = PodStatus.Running,
                GpuId = "gpu-1",
                Region = "US",
                CreatedAt = now,
            });

        await dbContext.AddPodPoolMemberAsync(
            new PodPoolMember
            {
                PodPoolId = poolId,
                GpuPodId = podId,
                State = state,
                JoinedAt = now,
            });
    }

    private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
