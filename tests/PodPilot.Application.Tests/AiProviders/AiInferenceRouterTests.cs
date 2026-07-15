using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.AiProviders;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.AiProviders;

public class AiInferenceRouterTests
{
    [Fact]
    public async Task TryResolveAsync_Returns_Null_When_No_Match()
    {
        await using var db = CreateDb();
        var service = new Mock<IAiProviderService>();
        var engine = new Mock<IRoutingEngine>();
        engine.Setup(e => e.RouteAsync(It.IsAny<RoutingEngineRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoutingDecision
            {
                DecisionReason = "none",
                Strategy = RoutingStrategy.Balanced,
            });

        var legacy = new LegacyAiInferenceRouter(db, service.Object, NullLogger<LegacyAiInferenceRouter>.Instance);
        var router = new AiInferenceRouter(engine.Object, legacy, db, service.Object);

        var route = await router.TryResolveAsync(Guid.NewGuid(), "unknown-model");
        Assert.Null(route);
    }

    [Fact]
    public async Task TryResolveAsync_Uses_Default_Routing_Policy()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        var provider = new AiInferenceProvider
        {
            OrganizationId = orgId,
            Name = "openai",
            DisplayName = "OpenAI",
            ProviderKind = AiProviderKind.OpenAi,
            IsEnabled = true,
            IsValidated = true,
            Credential = new AiProviderCredential { EncryptedApiKey = "enc" },
        };
        db.AiInferenceProviders.Add(provider);
        db.AiRoutingPolicies.Add(new AiRoutingPolicy
        {
            OrganizationId = orgId,
            Name = "default",
            PrimaryProviderId = provider.Id,
            IsEnabled = true,
            IsDefault = true,
            Strategy = RoutingStrategy.ProviderPriority,
            FailoverStrategy = AiFailoverStrategy.RetryThenFailover,
            MaxRetries = 1,
            FallbackProviderIdsJson = "[]",
            PreferredTaskTypesJson = "[]",
        });
        await db.SaveChangesAsync();

        var connection = new AiProviderConnection
        {
            OrganizationId = orgId,
            ProviderId = provider.Id,
            ProviderKind = AiProviderKind.OpenAi,
            ApiKey = "key",
            BaseUrl = "https://api.openai.com/v1",
        };
        var service = new Mock<IAiProviderService>();
        service.Setup(s => s.CreateConnectionAsync(It.IsAny<AiInferenceProvider>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var engine = new Mock<IRoutingEngine>();
        engine.Setup(e => e.RouteAsync(It.IsAny<RoutingEngineRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoutingDecision
            {
                DecisionReason = "none",
                Strategy = RoutingStrategy.ProviderPriority,
            });

        var legacy = new LegacyAiInferenceRouter(db, service.Object, NullLogger<LegacyAiInferenceRouter>.Instance);
        var router = new AiInferenceRouter(engine.Object, legacy, db, service.Object);
        var route = await router.TryResolveAsync(orgId, "gpt-4o");

        Assert.NotNull(route);
        Assert.Equal(provider.Id, route!.Connection.ProviderId);
        Assert.Equal("gpt-4o", route.Model);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"router-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
