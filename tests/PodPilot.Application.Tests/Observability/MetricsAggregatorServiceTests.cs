using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Observability;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Observability;

public class MetricsAggregatorServiceTests
{
  [Fact]
  public async Task Should_Aggregate_Live_Metrics_From_Orchestrator_And_Database()
  {
    var organizationId = Guid.NewGuid();
    var poolId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedPodWithMetricsAndStreamsAsync(dbContext, organizationId, poolId, podId, now);

    var orchestrator = new Mock<IPodOrchestrator>();
    orchestrator.Setup(o => o.GetStatusAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new OrchestratorStatus
      {
        RunningPods = 4,
        HealthyPods = 3,
        FailedPods = 1,
        QueueLength = 5,
        RequestsPerSecond = 2.5,
        AverageLatencyMs = 180,
      });

    var requestQueue = new Mock<IRequestQueue>();
    requestQueue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(7);

    var service = CreateService(dbContext, orchestrator.Object, requestQueue.Object, now);
    var snapshot = await service.GetLiveMetricsAsync(organizationId);

    Assert.Equal(now, snapshot.CapturedAt);
    Assert.Equal(75, snapshot.GpuUtilizationPercent);
    Assert.Equal(75, snapshot.CpuUtilizationPercent);
    Assert.Equal(12, snapshot.ActiveStreams);
    Assert.Equal(7, snapshot.QueueSize);
    Assert.Equal(2.5, snapshot.RequestsPerSecond);
    Assert.Equal(180, snapshot.AverageLatencyMs);
    Assert.Equal(4, snapshot.RunningPods);
    Assert.Equal(3, snapshot.HealthyPods);
    Assert.Equal(1, snapshot.FailedPods);
    Assert.Equal(1, snapshot.InferenceCountLastHour);
    Assert.Equal(500L, snapshot.TokensGeneratedLastHour);
  }

  [Fact]
  public async Task Should_Calculate_Error_Rate_From_Recent_Gateway_Requests()
  {
    var organizationId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = podId,
        OrganizationId = organizationId,
        ProviderId = Guid.NewGuid(),
        Name = "metrics-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        CreatedAt = now,
      });

    await AddGatewayRequestAsync(dbContext, organizationId, podId, GatewayRequestStatus.Completed, now.AddMinutes(-30));
    await AddGatewayRequestAsync(dbContext, organizationId, podId, GatewayRequestStatus.Failed, now.AddMinutes(-20));
    await AddGatewayRequestAsync(dbContext, organizationId, podId, GatewayRequestStatus.TimedOut, now.AddMinutes(-10));
    await dbContext.SaveChangesAsync();

    var orchestrator = new Mock<IPodOrchestrator>();
    orchestrator.Setup(o => o.GetStatusAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new OrchestratorStatus());

    var requestQueue = new Mock<IRequestQueue>();
    requestQueue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    var service = CreateService(dbContext, orchestrator.Object, requestQueue.Object, now);
    var snapshot = await service.GetLiveMetricsAsync(organizationId);

    Assert.Equal(2.0 / 3.0, snapshot.ErrorRate, precision: 5);
    Assert.Equal(1, snapshot.InferenceCountLastHour);
    Assert.Equal(500L, snapshot.TokensGeneratedLastHour);
  }

  [Fact]
  public async Task Should_Use_Recent_Pod_Health_Metrics_For_Gpu_Utilization()
  {
    var organizationId = Guid.NewGuid();
    var podOneId = Guid.NewGuid();
    var podTwoId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();

    foreach (var podId in new[] { podOneId, podTwoId })
    {
      await dbContext.AddGpuPodAsync(
        new GpuPod
        {
          Id = podId,
          OrganizationId = organizationId,
          ProviderId = Guid.NewGuid(),
          Name = $"pod-{podId:N}"[..12],
          Endpoint = "http://127.0.0.1:11434",
          Status = PodStatus.Running,
          GpuId = "gpu-1",
          Region = "US",
          CreatedAt = now,
        });
    }

    await dbContext.AddPodHealthMetricAsync(
      new PodHealthMetric
      {
        OrganizationId = organizationId,
        GpuPodId = podOneId,
        RecordedAt = now.AddMinutes(-5),
        GpuUtilizationPercent = 60,
      });

    await dbContext.AddPodHealthMetricAsync(
      new PodHealthMetric
      {
        OrganizationId = organizationId,
        GpuPodId = podTwoId,
        RecordedAt = now.AddMinutes(-3),
        GpuUtilizationPercent = 80,
      });

    await dbContext.SaveChangesAsync();

    var orchestrator = new Mock<IPodOrchestrator>();
    orchestrator.Setup(o => o.GetStatusAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new OrchestratorStatus { RunningPods = 2, HealthyPods = 2 });

    var requestQueue = new Mock<IRequestQueue>();
    requestQueue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    var service = CreateService(dbContext, orchestrator.Object, requestQueue.Object, now);
    var snapshot = await service.GetLiveMetricsAsync(organizationId);

    Assert.Equal(70, snapshot.GpuUtilizationPercent);
  }

  private static MetricsAggregatorService CreateService(
    ApplicationDbContext dbContext,
    IPodOrchestrator orchestrator,
    IRequestQueue requestQueue,
    DateTime utcNow) =>
    new(
      dbContext,
      orchestrator,
      requestQueue,
      new FixedDateTimeService(utcNow),
      NullLogger<MetricsAggregatorService>.Instance);

  private static ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase($"metrics-aggregator-{Guid.NewGuid()}")
      .Options;

    return new ApplicationDbContext(options);
  }

  private static async Task SeedPodWithMetricsAndStreamsAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid poolId,
    Guid podId,
    DateTime now)
  {
    await dbContext.AddPodPoolAsync(
      new PodPool
      {
        Id = poolId,
        OrganizationId = organizationId,
        Name = "metrics-pool",
        IsActive = true,
        CreatedAt = now,
      });

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = podId,
        OrganizationId = organizationId,
        ProviderId = Guid.NewGuid(),
        Name = "metrics-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        CreatedAt = now,
      });

    await dbContext.AddPodPoolMemberAsync(
      new PodPoolMember
      {
        PodPoolId = poolId,
        GpuPodId = podId,
        State = OrchestrationPodState.Healthy,
        JoinedAt = now,
        ActiveStreams = 12,
      });

    await dbContext.AddPodHealthMetricAsync(
      new PodHealthMetric
      {
        OrganizationId = organizationId,
        GpuPodId = podId,
        RecordedAt = now.AddMinutes(-5),
        GpuUtilizationPercent = 75,
      });

    await AddGatewayRequestAsync(
      dbContext,
      organizationId,
      podId,
      GatewayRequestStatus.Completed,
      now.AddMinutes(-15));

    await dbContext.SaveChangesAsync();
  }

  private static async Task AddGatewayRequestAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid podId,
    GatewayRequestStatus status,
    DateTime createdAt)
  {
    await dbContext.AddGatewayRequestAsync(
      new GatewayRequest
      {
        OrganizationId = organizationId,
        GpuPodId = podId,
        Status = status,
        HttpMethod = "POST",
        Path = "/v1/chat/completions",
        CreatedAt = createdAt,
        StartedAt = createdAt,
      });
  }

  private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
  {
    public DateTime UtcNow { get; } = utcNow;
  }
}
