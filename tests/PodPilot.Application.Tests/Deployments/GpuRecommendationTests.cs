using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Deployments;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Deployments;

public class GpuRecommendationTests
{
    [Fact]
    public async Task RecommendGpu_Returns_Recommended_For_Catalog_Model()
    {
        await using var db = CreateDb();
        var catalog = CreateCatalogService(db);

        var result = await catalog.RecommendGpuAsync(["qwen-coder-7b"]);

        Assert.Equal("RTX4090", result.RecommendedGpuCode);
        Assert.Equal("RTX4090", result.MinimumGpuCode);
        Assert.Equal(8, result.RequiredVramGb);
        Assert.Empty(result.Warnings);
        Assert.False(string.IsNullOrWhiteSpace(result.EstimatedPerformance));
    }

    [Fact]
    public async Task RecommendGpu_Uses_Max_Vram_Across_Models()
    {
        await using var db = CreateDb();
        var catalog = CreateCatalogService(db);

        var result = await catalog.RecommendGpuAsync(["llama32", "deepseek-r1"]);

        Assert.Equal(48, result.RequiredVramGb);
        Assert.Equal("L40S", result.MinimumGpuCode);
        Assert.Equal("L40S", result.RecommendedGpuCode);
    }

    [Fact]
    public async Task RecommendGpu_Warns_For_Unknown_Model()
    {
        await using var db = CreateDb();
        var catalog = CreateCatalogService(db);

        var result = await catalog.RecommendGpuAsync(["not-a-real-model"]);

        Assert.NotEmpty(result.Warnings);
        Assert.Contains(result.Warnings, w => w.Contains("not-a-real-model", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(24, result.RequiredVramGb);
    }

    private static DeploymentCatalogService CreateCatalogService(ApplicationDbContext db)
    {
        var dateTime = new FixedDateTimeService();
        var seeder = new DeploymentCatalogSeeder(db, dateTime);
        return new DeploymentCatalogService(db, seeder);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"deploy-catalog-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class FixedDateTimeService : IDateTimeService
    {
        public DateTime UtcNow => new(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);
    }
}
