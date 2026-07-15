using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Routing;

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

        var primaryModel = Guid.NewGuid();
        var fallbackModel = Guid.NewGuid();
        db.AiProviderModels.AddRange(
            new AiProviderModel
            {
                Id = primaryModel,
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
                Id = fallbackModel,
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

        var engine = new RoutingEngine(
            new TaskClassifier(),
            new ProviderSelector(db),
            new ModelRouter(),
            new RoutingPolicyService(db),
            new CostEstimator(db, clock.Object),
            new LatencyPredictor(db),
            new AvailabilityScorer(db),
            db,
            clock.Object,
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
