using Microsoft.EntityFrameworkCore;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Routing;

namespace PodPilot.Application.Tests.Routing;

public class LatencyPredictorTests
{
    [Fact]
    public async Task PredictAsync_Uses_Latency_History_Average()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        db.AiInferenceProviders.Add(new AiInferenceProvider
        {
            Id = providerId,
            OrganizationId = orgId,
            Name = "p",
            DisplayName = "P",
            ProviderKind = AiProviderKind.Groq,
            IsEnabled = true,
            IsValidated = true,
        });
        db.LatencyHistories.AddRange(
            new LatencyHistory
            {
                OrganizationId = orgId,
                AiProviderId = providerId,
                ModelName = "llama",
                LatencyMs = 200,
                RecordedAt = DateTime.UtcNow,
            },
            new LatencyHistory
            {
                OrganizationId = orgId,
                AiProviderId = providerId,
                ModelName = "llama",
                LatencyMs = 400,
                RecordedAt = DateTime.UtcNow,
            });
        await db.SaveChangesAsync();

        var predictor = new LatencyPredictor(db);
        var prediction = await predictor.PredictAsync(orgId, providerId, "llama");

        Assert.Equal(300, prediction.AverageResponseMs);
        Assert.True(prediction.PredictedLatencyMs >= prediction.AverageResponseMs);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"latency-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
