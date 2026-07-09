using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Orchestrator;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Orchestration;

public class PodPoolManagerTests
{
    [Fact]
    public async Task Should_Resolve_Pool_By_Model_Name()
    {
        var organizationId = Guid.NewGuid();
        var modelPoolId = Guid.NewGuid();
        var defaultPoolId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await SeedPoolsAsync(
            dbContext,
            organizationId,
            modelPoolId,
            defaultPoolId,
            modelName: "llama3",
            now);

        var manager = CreateManager(dbContext, now);
        var resolved = await manager.ResolvePoolAsync(organizationId, "llama3");

        Assert.NotNull(resolved);
        Assert.Equal(modelPoolId, resolved!.Id);
    }

    [Fact]
    public async Task Should_Resolve_Default_Pool_When_Model_Is_Missing()
    {
        var organizationId = Guid.NewGuid();
        var modelPoolId = Guid.NewGuid();
        var defaultPoolId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await SeedPoolsAsync(
            dbContext,
            organizationId,
            modelPoolId,
            defaultPoolId,
            modelName: "llama3",
            now);

        var manager = CreateManager(dbContext, now);
        var resolved = await manager.ResolvePoolAsync(organizationId, "mistral");

        Assert.NotNull(resolved);
        Assert.Equal(defaultPoolId, resolved!.Id);
        Assert.True(resolved.IsDefault);
    }

    [Fact]
    public async Task Should_Resolve_Default_Pool_When_Model_Name_Is_Null()
    {
        var organizationId = Guid.NewGuid();
        var modelPoolId = Guid.NewGuid();
        var defaultPoolId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await SeedPoolsAsync(
            dbContext,
            organizationId,
            modelPoolId,
            defaultPoolId,
            modelName: "llama3",
            now);

        var manager = CreateManager(dbContext, now);
        var resolved = await manager.ResolvePoolAsync(organizationId, null);

        Assert.NotNull(resolved);
        Assert.Equal(defaultPoolId, resolved!.Id);
    }

    [Fact]
    public async Task Should_Fallback_To_First_Active_Pool_When_No_Default_Exists()
    {
        var organizationId = Guid.NewGuid();
        var alphaPoolId = Guid.NewGuid();
        var betaPoolId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = betaPoolId,
                OrganizationId = organizationId,
                Name = "beta-pool",
                IsActive = true,
                IsDefault = false,
                CreatedAt = now,
            });
        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = alphaPoolId,
                OrganizationId = organizationId,
                Name = "alpha-pool",
                IsActive = true,
                IsDefault = false,
                CreatedAt = now,
            });
        await dbContext.SaveChangesAsync();

        var manager = CreateManager(dbContext, now);
        var resolved = await manager.ResolvePoolAsync(organizationId, "unknown-model");

        Assert.NotNull(resolved);
        Assert.Equal(alphaPoolId, resolved!.Id);
    }

    [Fact]
    public async Task Should_Exclude_Deleted_Pods_From_Healthy_Members()
    {
        var organizationId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var healthyPodId = Guid.NewGuid();
        var deletedPodId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var dbContext = CreateDbContext();
        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = poolId,
                OrganizationId = organizationId,
                Name = "member-pool",
                IsActive = true,
                CreatedAt = now,
            });

        await SeedPodMemberAsync(dbContext, poolId, healthyPodId, organizationId, PodStatus.Running, now);
        await SeedPodMemberAsync(dbContext, poolId, deletedPodId, organizationId, PodStatus.Deleted, now);
        await dbContext.SaveChangesAsync();

        var manager = CreateManager(dbContext, now);
        var members = await manager.GetHealthyMembersAsync(poolId);

        Assert.Single(members);
        Assert.Equal(healthyPodId, members[0].PodId);
    }

    private static PodPoolManager CreateManager(ApplicationDbContext dbContext, DateTime utcNow) =>
        new(
            dbContext,
            new NoOpOrchestratorNotificationService(),
            new FixedDateTimeService(utcNow),
            NullLogger<PodPoolManager>.Instance);

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"pod-pool-manager-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedPoolsAsync(
        ApplicationDbContext dbContext,
        Guid organizationId,
        Guid modelPoolId,
        Guid defaultPoolId,
        string modelName,
        DateTime now)
    {
        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = modelPoolId,
                OrganizationId = organizationId,
                Name = "model-pool",
                IsActive = true,
                IsDefault = false,
                CreatedAt = now,
            });
        await dbContext.AddPodPoolModelAsync(
            new PodPoolModel
            {
                PodPoolId = modelPoolId,
                ModelName = modelName,
            });

        await dbContext.AddPodPoolAsync(
            new PodPool
            {
                Id = defaultPoolId,
                OrganizationId = organizationId,
                Name = "default-pool",
                IsActive = true,
                IsDefault = true,
                CreatedAt = now,
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPodMemberAsync(
        ApplicationDbContext dbContext,
        Guid poolId,
        Guid podId,
        Guid organizationId,
        PodStatus status,
        DateTime now)
    {
        await dbContext.AddGpuPodAsync(
            new GpuPod
            {
                Id = podId,
                OrganizationId = organizationId,
                ProviderId = Guid.NewGuid(),
                Name = $"pod-{podId:N}"[..12],
                Endpoint = status == PodStatus.Deleted ? null : "http://127.0.0.1:11434",
                Status = status,
                GpuId = "gpu-1",
                Region = "US",
                CreatedAt = now,
            });

        await dbContext.AddPodPoolMemberAsync(
            new PodPoolMember
            {
                PodPoolId = poolId,
                GpuPodId = podId,
                State = status == PodStatus.Deleted
                    ? OrchestrationPodState.Failed
                    : OrchestrationPodState.Healthy,
                JoinedAt = now,
            });
    }

    private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
