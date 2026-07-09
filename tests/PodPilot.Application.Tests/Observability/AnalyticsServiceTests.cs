using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Observability;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Observability;

public class AnalyticsServiceTests
{
  [Fact]
  public async Task Should_Aggregate_Request_Counts_Tokens_Latency_And_Error_Rate()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);
    var periodStart = now.AddHours(-1);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodAsync(dbContext, organizationId, providerId, podId, now);

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.Completed,
      "llama3:latest",
      now.AddMinutes(-30),
      totalLatencyMs: 100);

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.Completed,
      "llama3:latest",
      now.AddMinutes(-20),
      totalLatencyMs: 200);

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.Failed,
      "llama3:latest",
      now.AddMinutes(-10));

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.TimedOut,
      "mistral:latest",
      now.AddMinutes(-5));

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, now);
    var summary = await service.GetAnalyticsAsync(
      organizationId,
      MetricsPeriod.Hourly,
      from: periodStart,
      to: now);

    Assert.Equal(MetricsPeriod.Hourly, summary.Period);
    Assert.Equal(4, summary.TotalRequests);
    Assert.Equal(2, summary.TotalInferences);
    Assert.Equal(1000L, summary.TotalTokens);
    Assert.Equal(150, summary.AverageLatencyMs);
    Assert.Equal(0.5, summary.ErrorRate);
  }

  [Fact]
  public async Task Should_Group_Requests_By_Model_And_Provider()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);
    var periodStart = now.AddDays(-1);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodAsync(dbContext, organizationId, providerId, podId, now, lastStartedAt: periodStart);

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.Completed,
      "llama3:latest",
      now.AddHours(-2),
      totalLatencyMs: 120);

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.Completed,
      "mistral:latest",
      now.AddHours(-1),
      totalLatencyMs: 80);

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, now);
    var summary = await service.GetAnalyticsAsync(
      organizationId,
      MetricsPeriod.Daily,
      from: periodStart,
      to: now);

    Assert.Equal(2, summary.ModelBreakdowns.Count);
    Assert.Equal("llama3:latest", summary.ModelBreakdowns[0].ModelName);
    Assert.Equal(1, summary.ModelBreakdowns[0].RequestCount);
    Assert.Equal(500L, summary.ModelBreakdowns[0].TokenCount);
    Assert.Equal(120, summary.ModelBreakdowns[0].AverageLatencyMs);

    Assert.Single(summary.ProviderBreakdowns);
    Assert.Equal(providerId, summary.ProviderBreakdowns[0].ProviderId);
    Assert.Equal(2, summary.ProviderBreakdowns[0].RequestCount);
    Assert.Equal(2, summary.ProviderBreakdowns[0].InferenceCount);

    Assert.Single(summary.PodBreakdowns);
    Assert.Equal(2, summary.PodBreakdowns[0].RequestCount);
    Assert.Equal(86400, summary.PodBreakdowns[0].UptimeSeconds);
  }

  [Fact]
  public async Task Should_Filter_By_Provider_Id()
  {
    var organizationId = Guid.NewGuid();
    var includedProviderId = Guid.NewGuid();
    var excludedProviderId = Guid.NewGuid();
    var includedPodId = Guid.NewGuid();
    var excludedPodId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);
    var periodStart = now.AddDays(-1);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodAsync(dbContext, organizationId, includedProviderId, includedPodId, now);
    await SeedProviderAndPodAsync(dbContext, organizationId, excludedProviderId, excludedPodId, now, providerName: "other-provider");

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      includedPodId,
      GatewayRequestStatus.Completed,
      "llama3:latest",
      now.AddHours(-1));

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      excludedPodId,
      GatewayRequestStatus.Completed,
      "llama3:latest",
      now.AddHours(-1));

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, now);
    var summary = await service.GetAnalyticsAsync(
      organizationId,
      MetricsPeriod.Daily,
      from: periodStart,
      to: now,
      providerId: includedProviderId);

    Assert.Equal(1, summary.TotalRequests);
    Assert.Single(summary.ProviderBreakdowns);
    Assert.Equal(includedProviderId, summary.ProviderBreakdowns[0].ProviderId);
  }

  [Fact]
  public async Task Should_Use_Persisted_Usage_Statistics_For_Uptime()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);
    var periodStart = now.AddDays(-1);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodAsync(dbContext, organizationId, providerId, podId, now);

    await dbContext.AddUsageStatisticsAsync(
      new UsageStatistics
      {
        OrganizationId = organizationId,
        RecordedAt = now.AddHours(-2),
        Period = MetricsPeriod.Daily,
        UptimeSeconds = 42_000,
      });

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, now);
    var summary = await service.GetAnalyticsAsync(
      organizationId,
      MetricsPeriod.Daily,
      from: periodStart,
      to: now);

    Assert.Equal(42_000, summary.TotalUptimeSeconds);
  }

  private static AnalyticsService CreateService(ApplicationDbContext dbContext, DateTime utcNow) =>
    new(
      dbContext,
      new FixedDateTimeService(utcNow),
      NullLogger<AnalyticsService>.Instance);

  private static ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase($"analytics-service-{Guid.NewGuid()}")
      .Options;

    return new ApplicationDbContext(options);
  }

  private static async Task SeedProviderAndPodAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid providerId,
    Guid podId,
    DateTime now,
    string providerName = "runpod",
    DateTime? lastStartedAt = null)
  {
    await dbContext.AddComputeProviderAsync(
      new ComputeProvider
      {
        Id = providerId,
        OrganizationId = organizationId,
        Name = providerName,
        DisplayName = providerName,
        ProviderType = ProviderType.RunPod,
        CreatedAt = now,
      });

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = podId,
        OrganizationId = organizationId,
        ProviderId = providerId,
        Name = $"pod-{podId:N}"[..12],
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        LastStartedAt = lastStartedAt,
        CreatedAt = now,
      });

    await dbContext.SaveChangesAsync();
  }

  private static async Task AddGatewayRequestAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid podId,
    GatewayRequestStatus status,
    string model,
    DateTime createdAt,
    int? totalLatencyMs = null)
  {
    var request = new GatewayRequest
    {
      OrganizationId = organizationId,
      GpuPodId = podId,
      Status = status,
      Model = model,
      HttpMethod = "POST",
      Path = "/v1/chat/completions",
      CreatedAt = createdAt,
      StartedAt = createdAt,
    };

    if (totalLatencyMs.HasValue)
    {
      request.Latency = new GatewayLatency
      {
        GatewayRequestId = request.Id,
        TotalLatencyMs = totalLatencyMs.Value,
        Request = request,
      };
    }

    await dbContext.AddGatewayRequestAsync(request);
  }

  private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
  {
    public DateTime UtcNow { get; } = utcNow;
  }
}
