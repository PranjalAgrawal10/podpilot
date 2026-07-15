using Microsoft.EntityFrameworkCore;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Routing;
using PodPilot.Infrastructure.Routing.Strategies;

namespace PodPilot.Application.Tests.Routing;

public class RoutingPolicyServiceTests
{
    [Fact]
    public async Task GetActivePolicyAsync_Returns_Default_Policy()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        db.AiRoutingPolicies.Add(new AiRoutingPolicy
        {
            OrganizationId = orgId,
            Name = "default",
            IsDefault = true,
            IsEnabled = true,
            Strategy = RoutingStrategy.LowestLatency,
            FallbackProviderIdsJson = "[]",
            PreferredTaskTypesJson = "[]",
        });
        await db.SaveChangesAsync();

        var service = new RoutingPolicyService(db);
        var policy = await service.GetActivePolicyAsync(orgId, null);

        Assert.NotNull(policy);
        Assert.Equal(RoutingStrategy.LowestLatency, policy!.Strategy);
    }

    [Fact]
    public void WeightResolver_Uses_LowestCost_Preset()
    {
        var resolver = new RoutingWeightResolver(
        [
            new LowestCostWeightStrategy(),
            new LowestLatencyWeightStrategy(),
            new HighestAccuracyWeightStrategy(),
            new PolicyConfiguredWeightStrategy(),
        ]);

        var weights = resolver.Resolve(null, RoutingStrategy.LowestCost);
        Assert.True(weights.Cost > weights.Latency);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"policy-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
