using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Routing;
using PodPilot.Infrastructure.Routing.Planners;
using PodPilot.Infrastructure.Routing.Strategies;

namespace PodPilot.Application.Tests.Routing;

public class RoutingEngineFallbackTests
{
    [Fact]
    public async Task RouteAsync_ProviderPriority_Orders_Primary_Then_Fallbacks()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        var primaryId = Guid.NewGuid();
        var fallbackId = Guid.NewGuid();

        db.AiInferenceProviders.AddRange(
            new AiInferenceProvider
            {
                Id = primaryId,
                OrganizationId = orgId,
                Name = "primary",
                DisplayName = "Primary",
                ProviderKind = AiProviderKind.OpenAi,
                IsEnabled = true,
                IsValidated = true,
                Priority = 1,
            },
            new AiInferenceProvider
            {
                Id = fallbackId,
                OrganizationId = orgId,
                Name = "fallback",
                DisplayName = "Fallback",
                ProviderKind = AiProviderKind.Groq,
                IsEnabled = true,
                IsValidated = true,
                Priority = 2,
            });

        db.AiProviderModels.AddRange(
            new AiProviderModel
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                AiProviderId = primaryId,
                ModelName = "model-a",
                IsEnabled = true,
                SyncedAt = DateTime.UtcNow,
                QualityScore = 80,
                SpeedScore = 70,
                ReliabilityScore = 80,
            },
            new AiProviderModel
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                AiProviderId = fallbackId,
                ModelName = "model-b",
                IsEnabled = true,
                SyncedAt = DateTime.UtcNow,
                QualityScore = 90,
                SpeedScore = 90,
                ReliabilityScore = 90,
            });

        db.AiRoutingPolicies.Add(new AiRoutingPolicy
        {
            OrganizationId = orgId,
            Name = "priority",
            IsDefault = true,
            IsEnabled = true,
            Strategy = RoutingStrategy.ProviderPriority,
            PrimaryProviderId = primaryId,
            FallbackProviderIdsJson = $"[\"{fallbackId}\"]",
            PreferredTaskTypesJson = "[]",
            FailoverStrategy = AiFailoverStrategy.RetryThenFailover,
            MaxRetries = 1,
        });
        await db.SaveChangesAsync();

        var clock = new Mock<IDateTimeService>();
        clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);

        var scorer = new ModelScorer();
        var weightResolver = new RoutingWeightResolver(
        [
            new LowestCostWeightStrategy(),
            new LowestLatencyWeightStrategy(),
            new HighestAccuracyWeightStrategy(),
            new PolicyConfiguredWeightStrategy(),
        ]);
        var enricher = new RoutingCandidateEnricher(
            new CostEstimator(db, clock.Object, new ProviderCostRateCatalog()),
            new LatencyPredictor(db),
            new AvailabilityScorer(db));
        var providerSelector = new ProviderSelector(db);
        var planners = new IRoutePlanner[]
        {
            new ProviderPriorityRoutePlanner(providerSelector, enricher),
            new ScoredRoutePlanner(
                providerSelector,
                enricher,
                weightResolver,
                scorer,
                new ModelRouter(scorer)),
        };

        var engine = new RoutingEngine(
            new TaskClassifier(),
            new RoutingPolicyService(db),
            planners,
            new RoutingDecisionStore(db, clock.Object),
            new NoOpRoutingNotificationService(),
            NullLogger<RoutingEngine>.Instance);

        var decision = await engine.RouteAsync(new RoutingEngineRequest
        {
            OrganizationId = orgId,
            Prompt = "hello world",
            Path = "/v1/chat/completions",
        });

        Assert.NotNull(decision.Selected);
        Assert.Equal(primaryId, decision.Selected!.ProviderId);
        Assert.Contains(decision.Fallbacks, f => f.ProviderId == fallbackId);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"engine-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
