using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Observability;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Observability;

public class MonitoringServiceTests
{
  [Fact]
  public async Task Should_Raise_HighGpuUsage_Alert_When_Threshold_Exceeded()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedHealthyPodAsync(dbContext, organizationId, providerId, podId, now, gpuUtilizationPercent: 95);

    var service = CreateService(dbContext, CreateDefaultOrchestrator(organizationId), CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.HighGpuUsage && a.GpuPodId == podId);
  }

  [Fact]
  public async Task Should_Raise_HighQueueLength_Alert_When_Queue_Exceeds_Threshold()
  {
    var organizationId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();

    var orchestrator = new Mock<IPodOrchestrator>();
    orchestrator.Setup(o => o.GetStatusAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new OrchestratorStatus { QueueLength = 25 });

    var service = CreateService(dbContext, orchestrator.Object, CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.HighQueueLength);
  }

  [Fact]
  public async Task Should_Raise_HighLatency_Alert_When_Latency_Exceeds_Threshold()
  {
    var organizationId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();

    var orchestrator = new Mock<IPodOrchestrator>();
    orchestrator.Setup(o => o.GetStatusAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new OrchestratorStatus { AverageLatencyMs = 6000 });

    var service = CreateService(dbContext, orchestrator.Object, CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.HighLatency);
  }

  [Fact]
  public async Task Should_Raise_PodFailure_Alert_For_Unhealthy_Pod()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAsync(dbContext, organizationId, providerId, now);

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = podId,
        OrganizationId = organizationId,
        ProviderId = providerId,
        Name = "failed-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Failed,
        GpuId = "gpu-1",
        Region = "US",
        CreatedAt = now,
      });

    await dbContext.AddPodHealthMetricAsync(
      new PodHealthMetric
      {
        OrganizationId = organizationId,
        GpuPodId = podId,
        RecordedAt = now.AddMinutes(-5),
        State = OrchestrationPodState.Failed,
        OllamaHealthy = false,
        ModelsHealthy = false,
        ErrorMessage = "Pod crashed",
      });

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, CreateDefaultOrchestrator(organizationId), CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.PodFailure && a.GpuPodId == podId);
  }

  [Fact]
  public async Task Should_Raise_ProviderFailure_Alert_For_Disconnected_Provider()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAsync(dbContext, organizationId, providerId, now);

    await dbContext.AddProviderHealthAsync(
      new ProviderHealth
      {
        ComputeProviderId = providerId,
        Status = ProviderConnectionStatus.Disconnected,
        ErrorMessage = "API unreachable",
        LastCheckedAt = now.AddMinutes(-1),
      });

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, CreateDefaultOrchestrator(organizationId), CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.ProviderFailure && a.ProviderId == providerId);
  }

  [Fact]
  public async Task Should_Raise_RepeatedGatewayErrors_Alert_When_Error_Count_Exceeds_Threshold()
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
        Name = "gateway-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        CreatedAt = now,
      });

    for (var i = 0; i < 10; i++)
    {
      await dbContext.AddGatewayRequestAsync(
        new GatewayRequest
        {
          OrganizationId = organizationId,
          GpuPodId = podId,
          Status = GatewayRequestStatus.Failed,
          HttpMethod = "POST",
          Path = "/v1/chat/completions",
          CreatedAt = now.AddMinutes(-i),
          StartedAt = now.AddMinutes(-i),
        });
    }

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, CreateDefaultOrchestrator(organizationId), CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.RepeatedGatewayErrors);
  }

  [Fact]
  public async Task Should_Raise_ModelFailure_Alert_When_Models_Unhealthy_But_Ollama_Healthy()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAsync(dbContext, organizationId, providerId, now);

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = podId,
        OrganizationId = organizationId,
        ProviderId = providerId,
        Name = "model-issue-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        CreatedAt = now,
      });

    await dbContext.AddPodHealthMetricAsync(
      new PodHealthMetric
      {
        OrganizationId = organizationId,
        GpuPodId = podId,
        RecordedAt = now.AddMinutes(-5),
        OllamaHealthy = true,
        ModelsHealthy = false,
        GpuHealthy = true,
        State = OrchestrationPodState.Healthy,
      });

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, CreateDefaultOrchestrator(organizationId), CreateDefaultQueue(organizationId), now);
    var alerts = await service.EvaluateAlertsAsync(organizationId);

    Assert.Contains(alerts, a => a.AlertType == AlertType.ModelFailure && a.GpuPodId == podId);
  }

  private static MonitoringService CreateService(
    ApplicationDbContext dbContext,
    IPodOrchestrator orchestrator,
    IRequestQueue requestQueue,
    DateTime utcNow,
    IObservabilityNotificationService? notificationService = null) =>
    new(
      dbContext,
      orchestrator,
      requestQueue,
      new FixedDateTimeService(utcNow),
      notificationService ?? new NoOpObservabilityNotificationService(),
      NullLogger<MonitoringService>.Instance);

  private static IPodOrchestrator CreateDefaultOrchestrator(Guid organizationId)
  {
    var orchestrator = new Mock<IPodOrchestrator>();
    orchestrator.Setup(o => o.GetStatusAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new OrchestratorStatus());
    return orchestrator.Object;
  }

  private static IRequestQueue CreateDefaultQueue(Guid organizationId)
  {
    var requestQueue = new Mock<IRequestQueue>();
    requestQueue.Setup(q => q.GetLengthAsync(organizationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);
    return requestQueue.Object;
  }

  private static ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase($"monitoring-service-{Guid.NewGuid()}")
      .Options;

    return new ApplicationDbContext(options);
  }

  private static async Task SeedProviderAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid providerId,
    DateTime now)
  {
    await dbContext.AddComputeProviderAsync(
      new ComputeProvider
      {
        Id = providerId,
        OrganizationId = organizationId,
        Name = "runpod",
        DisplayName = "RunPod",
        ProviderType = ProviderType.RunPod,
        CreatedAt = now,
      });

    await dbContext.SaveChangesAsync();
  }

  private static async Task SeedHealthyPodAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid providerId,
    Guid podId,
    DateTime now,
    double gpuUtilizationPercent)
  {
    await SeedProviderAsync(dbContext, organizationId, providerId, now);

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = podId,
        OrganizationId = organizationId,
        ProviderId = providerId,
        Name = "healthy-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        CreatedAt = now,
      });

    await dbContext.AddPodHealthMetricAsync(
      new PodHealthMetric
      {
        OrganizationId = organizationId,
        GpuPodId = podId,
        RecordedAt = now.AddMinutes(-5),
        GpuUtilizationPercent = gpuUtilizationPercent,
        OllamaHealthy = true,
        ModelsHealthy = true,
        GpuHealthy = true,
        State = OrchestrationPodState.Healthy,
      });

    await dbContext.SaveChangesAsync();
  }

  private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
  {
    public DateTime UtcNow { get; } = utcNow;
  }
}
