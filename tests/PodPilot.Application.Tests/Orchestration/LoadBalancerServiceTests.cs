using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Orchestrator;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Orchestration;

public class LoadBalancerServiceTests
{
    [Theory]
    [InlineData(LoadBalancingStrategy.LeastBusy)]
    [InlineData(LoadBalancingStrategy.LeastQueue)]
    [InlineData(LoadBalancingStrategy.LowestLatency)]
    [InlineData(LoadBalancingStrategy.RoundRobin)]
    [InlineData(LoadBalancingStrategy.Weighted)]
    [InlineData(LoadBalancingStrategy.StickySession)]
    public async Task Should_Select_Eligible_Pod_For_Strategy(LoadBalancingStrategy strategy)
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var expectedPodId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedLoadBalancerConfigAsync(dbContext, organizationId, strategy);

        var service = CreateService(dbContext);
        var members = CreateMembers(expectedPodId, poolId, organizationId);

        var selection = await service.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = organizationId,
                PoolId = poolId,
                Members = members,
            });

        Assert.NotNull(selection);
        Assert.Equal(expectedPodId, selection!.PodId);
        Assert.Equal(strategy, selection.Strategy);
    }

    [Fact]
    public async Task Should_Select_LeastBusy_Pod_When_Loads_Differ()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var leastBusyPodId = Guid.NewGuid();
        var busyPodId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedLoadBalancerConfigAsync(dbContext, organizationId, LoadBalancingStrategy.LeastBusy);

        var service = CreateService(dbContext);
        var members = new[]
        {
            CreateMember(leastBusyPodId, organizationId, currentLoad: 1, queueDepth: 0, averageLatencyMs: 100),
            CreateMember(busyPodId, organizationId, currentLoad: 3, queueDepth: 0, averageLatencyMs: 50),
        };

        var selection = await service.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = organizationId,
                PoolId = poolId,
                Members = members,
            });

        Assert.NotNull(selection);
        Assert.Equal(leastBusyPodId, selection!.PodId);
    }

    [Fact]
    public async Task Should_Select_LeastQueue_Pod_When_Queue_Depths_Differ()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var shortestQueuePodId = Guid.NewGuid();
        var deepQueuePodId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedLoadBalancerConfigAsync(dbContext, organizationId, LoadBalancingStrategy.LeastQueue);

        var service = CreateService(dbContext);
        var members = new[]
        {
            CreateMember(shortestQueuePodId, organizationId, currentLoad: 2, queueDepth: 1, averageLatencyMs: 100),
            CreateMember(deepQueuePodId, organizationId, currentLoad: 1, queueDepth: 8, averageLatencyMs: 50),
        };

        var selection = await service.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = organizationId,
                PoolId = poolId,
                Members = members,
            });

        Assert.NotNull(selection);
        Assert.Equal(shortestQueuePodId, selection!.PodId);
    }

    [Fact]
    public async Task Should_Select_LowestLatency_Pod_When_Latencies_Differ()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var fastPodId = Guid.NewGuid();
        var slowPodId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedLoadBalancerConfigAsync(dbContext, organizationId, LoadBalancingStrategy.LowestLatency);

        var service = CreateService(dbContext);
        var members = new[]
        {
            CreateMember(fastPodId, organizationId, currentLoad: 2, queueDepth: 2, averageLatencyMs: 40),
            CreateMember(slowPodId, organizationId, currentLoad: 1, queueDepth: 1, averageLatencyMs: 250),
        };

        var selection = await service.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = organizationId,
                PoolId = poolId,
                Members = members,
            });

        Assert.NotNull(selection);
        Assert.Equal(fastPodId, selection!.PodId);
    }

    [Fact]
    public async Task Should_Return_Null_When_All_Members_Are_Overloaded()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedLoadBalancerConfigAsync(dbContext, organizationId, LoadBalancingStrategy.LeastBusy);

        var service = CreateService(dbContext);
        var members = new[]
        {
            CreateMember(
                Guid.NewGuid(),
                organizationId,
                currentLoad: ApplicationConstants.SchedulerMaxConcurrentPerPod,
                queueDepth: 0,
                averageLatencyMs: 10),
            CreateMember(
                Guid.NewGuid(),
                organizationId,
                currentLoad: ApplicationConstants.SchedulerMaxConcurrentPerPod + 1,
                queueDepth: 0,
                averageLatencyMs: 10),
        };

        var selection = await service.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = organizationId,
                PoolId = poolId,
                Members = members,
            });

        Assert.Null(selection);
    }

    [Fact]
    public async Task Should_Fallback_To_WarmStandby_When_No_Active_Members_Are_Available()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var warmStandbyPodId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedLoadBalancerConfigAsync(dbContext, organizationId, LoadBalancingStrategy.LeastBusy);

        var service = CreateService(dbContext);
        var members = new[]
        {
            CreateMember(
                warmStandbyPodId,
                organizationId,
                currentLoad: 0,
                queueDepth: 0,
                averageLatencyMs: 10,
                isWarmStandby: true),
        };

        var selection = await service.SelectPodAsync(
            new LoadBalancerRequest
            {
                OrganizationId = organizationId,
                PoolId = poolId,
                Members = members,
            });

        Assert.NotNull(selection);
        Assert.Equal(warmStandbyPodId, selection!.PodId);
    }

    private static LoadBalancerService CreateService(IApplicationDbContext dbContext) =>
        new(
            dbContext,
            new FixedDateTimeService(DateTime.UtcNow),
            NullLogger<LoadBalancerService>.Instance);

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"load-balancer-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedLoadBalancerConfigAsync(
        ApplicationDbContext dbContext,
        Guid organizationId,
        LoadBalancingStrategy strategy)
    {
        await dbContext.AddLoadBalancerConfigAsync(
            new LoadBalancerConfig
            {
                OrganizationId = organizationId,
                Strategy = strategy,
                StickySessionsEnabled = false,
                StickySessionTtlMinutes = 30,
                CreatedAt = DateTime.UtcNow,
            });
        await dbContext.SaveChangesAsync();
    }

    private static IReadOnlyList<PoolMemberContext> CreateMembers(
        Guid podId,
        Guid poolId,
        Guid organizationId) =>
    [
        CreateMember(podId, organizationId, currentLoad: 0, queueDepth: 0, averageLatencyMs: 25),
    ];

    private static PoolMemberContext CreateMember(
        Guid podId,
        Guid organizationId,
        int currentLoad,
        int queueDepth,
        int averageLatencyMs,
        bool isWarmStandby = false) =>
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
            CurrentLoad = currentLoad,
            QueueDepth = queueDepth,
            AverageLatencyMs = averageLatencyMs,
            IsWarmStandby = isWarmStandby,
            Weight = 1,
        };

    private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
