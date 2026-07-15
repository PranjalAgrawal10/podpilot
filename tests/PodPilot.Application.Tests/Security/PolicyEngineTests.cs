using Microsoft.EntityFrameworkCore;
using Moq;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Security;

namespace PodPilot.Application.Tests.Security;

public class PolicyEngineTests
{
    [Fact]
    public async Task EnsureProviderAllowedAsync_Blocks_Disallowed_Provider()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        await db.AddOrganizationGovernancePolicyAsync(new OrganizationGovernancePolicy
        {
            OrganizationId = orgId,
            AllowedProvidersJson = "[\"OpenAi\"]",
            AllowedModelsJson = "[]",
            AllowedPluginsJson = "[]",
            AllowedMcpServersJson = "[]",
            EmptyAllowListMeansAllowAll = false,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var engine = new PolicyEngine(db, Mock.Of<ISecurityNotificationService>());
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            engine.EnsureProviderAllowedAsync(orgId, "Anthropic"));
    }

    [Fact]
    public async Task EnsureIpAllowedAsync_Blocks_Non_Allowlisted_Ip()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        await db.AddOrganizationSecurityPolicyAsync(new OrganizationSecurityPolicy
        {
            OrganizationId = orgId,
            IpAllowListJson = "[\"10.0.0.1\"]",
            GeoAllowListJson = "[]",
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var engine = new PolicyEngine(db, Mock.Of<ISecurityNotificationService>());
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            engine.EnsureIpAllowedAsync(orgId, "192.168.1.1"));
    }

    [Fact]
    public async Task EnsurePluginAllowedAsync_Allows_When_Empty_List_Means_All()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        var engine = new PolicyEngine(db, Mock.Of<ISecurityNotificationService>());
        await engine.EnsurePluginAllowedAsync(orgId, "com.podpilot.utility.health");
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"policy-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
