using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Routing;

namespace PodPilot.Application.Tests.Routing;

public class ModelRouterTests
{
    [Fact]
    public async Task SelectModelAsync_Prefers_Lower_Cost_When_LowestCost_Weights()
    {
        var candidates = new List<RoutingCandidate>
        {
            new()
            {
                ProviderId = Guid.NewGuid(),
                ProviderName = "Cheap",
                ModelId = Guid.NewGuid(),
                ModelName = "cheap-model",
                PredictedCostUsd = 0.001m,
                PredictedLatencyMs = 900,
                ReliabilityScore = 70,
                ContextLength = 8000,
                QualityScore = 60,
                SpeedScore = 60,
                AvailabilityScore = 80,
            },
            new()
            {
                ProviderId = Guid.NewGuid(),
                ProviderName = "Fast",
                ModelId = Guid.NewGuid(),
                ModelName = "fast-model",
                PredictedCostUsd = 0.05m,
                PredictedLatencyMs = 100,
                ReliabilityScore = 80,
                ContextLength = 32000,
                QualityScore = 80,
                SpeedScore = 90,
                AvailabilityScore = 90,
            },
        };

        var router = new ModelRouter();
        var selected = await router.SelectModelAsync(
            candidates,
            new RoutingRequestAnalysis { TaskType = AiTaskType.Chat, Complexity = TaskComplexity.Low },
            RoutingScoreWeights.LowestCost);

        Assert.NotNull(selected);
        Assert.Equal("cheap-model", selected!.ModelName);
    }

    [Fact]
    public async Task SelectModelAsync_Returns_Null_When_Empty()
    {
        var router = new ModelRouter();
        var selected = await router.SelectModelAsync(
            [],
            new RoutingRequestAnalysis(),
            RoutingScoreWeights.Balanced);

        Assert.Null(selected);
    }
}
