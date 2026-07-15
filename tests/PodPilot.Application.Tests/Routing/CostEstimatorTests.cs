using Microsoft.EntityFrameworkCore;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Routing;

namespace PodPilot.Application.Tests.Routing;

public class CostEstimatorTests
{
    [Fact]
    public async Task EstimateAsync_Computes_Token_Costs()
    {
        await using var db = CreateDb();
        var clock = new Mock<IDateTimeService>();
        clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));

        var estimator = new CostEstimator(db, clock.Object);
        var estimate = await estimator.EstimateAsync(
            new RoutingCandidate
            {
                ProviderId = Guid.NewGuid(),
                ProviderKind = AiProviderKind.OpenAi,
                ModelName = "gpt-4o-mini",
                InputCostPerMillionTokens = 0.15m,
                OutputCostPerMillionTokens = 0.60m,
                SpeedScore = 70,
            },
            inputTokens: 1_000_000,
            outputTokens: 1_000_000,
            organizationId: Guid.NewGuid());

        Assert.Equal(0.15m, estimate.InputCostUsd);
        Assert.Equal(0.60m, estimate.OutputCostUsd);
        Assert.Equal(0.75m, estimate.TotalCostUsd);
        Assert.True(estimate.GpuRuntimeMs > 0);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"cost-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
