using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Orchestrator;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Orchestration;

public class CapacityPlannerServiceTests
{
    [Fact]
    public async Task Should_Calculate_Current_And_Projected_Capacity()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var podOneId = Guid.NewGuid();
        var podTwoId = Guid.NewGuid();
        var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

        await using var dbContext = CreateDbContext();
        await SeedPoolWithMembersAsync(dbContext, organizationId, poolId, podOneId, podTwoId, now);
        await SeedActiveRequestsAsync(dbContext, organizationId, podOneId, podTwoId, activeCount: 6, now);

        var queue = new Mock<IRequestQueue>();
        queue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        var podPoolManager = new Mock<IPodPoolManager>();
        podPoolManager.Setup(m => m.GetHealthyMembersAsync(poolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                CreateHealthyMember(podOneId, organizationId, averageLatencyMs: 100),
                CreateHealthyMember(podTwoId, organizationId, averageLatencyMs: 200),
            ]);

        var service = CreateService(dbContext, podPoolManager.Object, queue.Object, now);
        var plan = await service.CalculateAsync(organizationId, poolId);

        const int maxCapacity = 2 * ApplicationConstants.SchedulerMaxConcurrentPerPod;
        var expectedCurrentCapacity = 6.0 / maxCapacity;
        var expectedProjectedCapacity = Math.Min(1.0, expectedCurrentCapacity + (4.0 / maxCapacity));

        Assert.Equal(2, plan.TotalPods);
        Assert.Equal(2, plan.HealthyPods);
        Assert.Equal(0, plan.BusyPods);
        Assert.Equal(4, plan.QueueLength);
        Assert.Equal(expectedCurrentCapacity, plan.CurrentCapacity, precision: 5);
        Assert.Equal(expectedProjectedCapacity, plan.ProjectedCapacity, precision: 5);
        Assert.Equal(1.0 - expectedCurrentCapacity, plan.RemainingCapacity, precision: 5);
        Assert.Equal(150, plan.AverageLatencyMs);
        Assert.Equal(600, plan.AverageWaitTimeMs);
        Assert.Equal(4.0, plan.MaximumThroughput);
        Assert.Equal(1, plan.SuggestedScale);
    }

    [Fact]
    public async Task Should_Suggest_ScaleDown_When_Underutilized_With_Multiple_Pods()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var podOneId = Guid.NewGuid();
        var podTwoId = Guid.NewGuid();
        var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

        await using var dbContext = CreateDbContext();
        await SeedPoolWithMembersAsync(dbContext, organizationId, poolId, podOneId, podTwoId, now);
        await SeedActiveRequestsAsync(dbContext, organizationId, podOneId, podTwoId, activeCount: 1, now);

        var queue = new Mock<IRequestQueue>();
        queue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var podPoolManager = new Mock<IPodPoolManager>();
        podPoolManager.Setup(m => m.GetHealthyMembersAsync(poolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                CreateHealthyMember(podOneId, organizationId, averageLatencyMs: 0),
                CreateHealthyMember(podTwoId, organizationId, averageLatencyMs: 0),
            ]);

        var service = CreateService(dbContext, podPoolManager.Object, queue.Object, now);
        var plan = await service.CalculateAsync(organizationId, poolId);

        Assert.Equal(2, plan.TotalPods);
        Assert.True(plan.CurrentCapacity <= 0.3);
        Assert.Equal(0, plan.QueueLength);
        Assert.Equal(-1, plan.SuggestedScale);
    }

    [Fact]
    public async Task Should_Use_Health_Metrics_For_Gpu_Utilization_When_Available()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var podId = Guid.NewGuid();
        var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

        await using var dbContext = CreateDbContext();
        await SeedPoolWithMembersAsync(dbContext, organizationId, poolId, podId, podId, now, memberCount: 1);
        await dbContext.AddPodHealthMetricAsync(
            new PodHealthMetric
            {
                OrganizationId = organizationId,
                GpuPodId = podId,
                RecordedAt = now.AddMinutes(-5),
                GpuUtilizationPercent = 72.5,
                LatencyMs = 80,
            });
        await dbContext.SaveChangesAsync();

        var queue = new Mock<IRequestQueue>();
        queue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var podPoolManager = new Mock<IPodPoolManager>();
        podPoolManager.Setup(m => m.GetHealthyMembersAsync(poolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateHealthyMember(podId, organizationId, averageLatencyMs: 80)]);

        var service = CreateService(dbContext, podPoolManager.Object, queue.Object, now);
        var plan = await service.CalculateAsync(organizationId, poolId);

        Assert.Equal(72.5, plan.GpuUtilizationPercent);
    }

    private static CapacityPlannerService CreateService(
        ApplicationDbContext dbContext,
        IPodPoolManager podPoolManager,
        IRequestQueue requestQueue,
        DateTime utcNow) =>
        new(
            dbContext,
            podPoolManager,
            requestQueue,
            new FixedDateTimeService(utcNow),
            NullLogger<CapacityPlannerService>.Instance);

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"capacity-planner-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedPoolWithMembersAsync(
        ApplicationDbContext dbContext,
        Guid organizationId,
        Guid poolId,
        Guid podOneId,
        Guid podTwoId,
        DateTime now,
        int memberCount = 2)
    {
        var pool = new PodPool
        {
            Id = poolId,
            OrganizationId = organizationId,
            Name = "test-pool",
            IsActive = true,
            CreatedAt = now,
        };

        await dbContext.AddPodPoolAsync(pool);

        var podIds = memberCount == 1 ? [podOneId] : new[] { podOneId, podTwoId };
        foreach (var podId in podIds)
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
                    State = OrchestrationPodState.Healthy,
                    JoinedAt = now,
                });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedActiveRequestsAsync(
        ApplicationDbContext dbContext,
        Guid organizationId,
        Guid podOneId,
        Guid podTwoId,
        int activeCount,
        DateTime now)
    {
        for (var i = 0; i < activeCount; i++)
        {
            var podId = i % 2 == 0 ? podOneId : podTwoId;
            await dbContext.AddGatewayRequestAsync(
                new GatewayRequest
                {
                    OrganizationId = organizationId,
                    GpuPodId = podId,
                    Status = GatewayRequestStatus.Forwarding,
                    HttpMethod = "POST",
                    Path = "/v1/chat/completions",
                    CreatedAt = now,
                    StartedAt = now,
                });
        }

        await dbContext.SaveChangesAsync();
    }

    private static PoolMemberContext CreateHealthyMember(
        Guid podId,
        Guid organizationId,
        int averageLatencyMs) =>
        new()
        {
            MemberId = Guid.NewGuid(),
            PodId = podId,
            Pod = new GpuPod
            {
                Id = podId,
                OrganizationId = organizationId,
                Name = $"pod-{podId:N}"[..12],
                Endpoint = "http://127.0.0.1:11434",
                Status = PodStatus.Running,
                GpuId = "gpu-1",
                Region = "US",
                ProviderId = Guid.NewGuid(),
            },
            BaseUrl = "http://127.0.0.1:11434",
            State = OrchestrationPodState.Healthy,
            AverageLatencyMs = averageLatencyMs,
        };

    private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
