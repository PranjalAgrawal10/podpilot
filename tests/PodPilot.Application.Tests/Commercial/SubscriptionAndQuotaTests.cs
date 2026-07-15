using Microsoft.EntityFrameworkCore;
using Moq;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Commercial;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Commercial;

public class SubscriptionAndQuotaTests
{
    [Fact]
    public async Task EnsureCatalog_Seeds_Four_Plans()
    {
        await using var db = CreateDb();
        var service = CreateSubscriptionService(db);
        await service.EnsureCatalogAsync();

        var plans = await db.SubscriptionPlans.Include(p => p.Quota).ToListAsync();
        Assert.Equal(4, plans.Count);
        Assert.Contains(plans, p => p.Code == "free");
        Assert.Contains(plans, p => p.Code == "enterprise");
        Assert.All(plans, p => Assert.NotNull(p.Quota));
    }

    [Fact]
    public async Task GetOrCreate_Assigns_Free_Plan()
    {
        await using var db = CreateDb();
        var orgId = await SeedOrganizationAsync(db);
        var service = CreateSubscriptionService(db);
        var sub = await service.GetOrCreateAsync(orgId);

        Assert.Equal("free", sub.PlanCode);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task QuotaService_Blocks_When_Over_Pod_Limit()
    {
        await using var db = CreateDb();
        var orgId = await SeedOrganizationAsync(db);
        var subscriptionService = CreateSubscriptionService(db);
        await subscriptionService.GetOrCreateAsync(orgId);

        // Free plan MaxPods = 2
        for (var i = 0; i < 2; i++)
        {
            await db.GpuPods.AddAsync(new GpuPod
            {
                OrganizationId = orgId,
                Name = $"pod-{i}",
                GpuId = "gpu",
                Region = "us",
                ImageName = "ollama",
                Status = PodStatus.Running,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            new QuotaService(db, subscriptionService).EnsureCanCreatePodAsync(orgId));
    }

    private static async Task<Guid> SeedOrganizationAsync(ApplicationDbContext db)
    {
        var orgId = Guid.NewGuid();
        await db.Organizations.AddAsync(new Organization
        {
            Id = orgId,
            Name = "Acme",
            Slug = $"acme-{orgId:N}"[..20],
            OwnerUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        return orgId;
    }

    private static SubscriptionService CreateSubscriptionService(ApplicationDbContext db) =>
        new(
            db,
            new PlanCatalogSeeder(db),
            Mock.Of<IPaymentGatewayFactory>(),
            Mock.Of<ICommercialNotificationService>(),
            Mock.Of<ICurrentUserService>());

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"commercial-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
