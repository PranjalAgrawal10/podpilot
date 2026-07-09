using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Observability;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Observability;

public class CostCalculatorServiceTests
{
  [Theory]
  [InlineData(MetricsPeriod.Hourly, 1)]
  [InlineData(MetricsPeriod.Daily, 24)]
  [InlineData(MetricsPeriod.Monthly, 720)]
  public async Task Should_Calculate_Period_Cost_For_Running_Pods(MetricsPeriod period, decimal periodHours)
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podOneId = Guid.NewGuid();
    var podTwoId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodsAsync(
      dbContext,
      organizationId,
      providerId,
      podOneId,
      podTwoId,
      now,
      hourlyCosts: [1.50m, 0.50m]);

    var service = CreateService(dbContext, now);
    var summary = await service.CalculateAsync(organizationId, period);

    const decimal expectedHourlyCost = 2.00m;

    Assert.Equal(period, summary.Period);
    Assert.Equal(expectedHourlyCost, summary.HourlyCost);
    Assert.Equal(expectedHourlyCost * 24m, summary.DailyCost);
    Assert.Equal(expectedHourlyCost * 24m * 7m, summary.WeeklyCost);
    Assert.Equal(expectedHourlyCost * 24m * 30m, summary.MonthlyCost);
    Assert.Equal(expectedHourlyCost * periodHours, summary.PodBreakdowns.Sum(p => p.PeriodCost));
    Assert.Equal(2, summary.PodBreakdowns.Count);
    Assert.Single(summary.ProviderBreakdowns);
    Assert.Equal(expectedHourlyCost, summary.ProviderBreakdowns[0].HourlyCost);
    Assert.Equal(expectedHourlyCost * periodHours, summary.ProviderBreakdowns[0].PeriodCost);
  }

  [Fact]
  public async Task Should_Exclude_Stopped_Pods_From_Cost()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var runningPodId = Guid.NewGuid();
    var stoppedPodId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAsync(dbContext, organizationId, providerId, now);

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = runningPodId,
        OrganizationId = organizationId,
        ProviderId = providerId,
        Name = "running-pod",
        Endpoint = "http://127.0.0.1:11434",
        Status = PodStatus.Running,
        GpuId = "gpu-1",
        Region = "US",
        HourlyCost = 1.00m,
        CreatedAt = now,
      });

    await dbContext.AddGpuPodAsync(
      new GpuPod
      {
        Id = stoppedPodId,
        OrganizationId = organizationId,
        ProviderId = providerId,
        Name = "stopped-pod",
        Endpoint = "http://127.0.0.1:11435",
        Status = PodStatus.Stopped,
        GpuId = "gpu-1",
        Region = "US",
        HourlyCost = 5.00m,
        CreatedAt = now,
      });

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, now);
    var summary = await service.CalculateAsync(organizationId, MetricsPeriod.Hourly);

    Assert.Equal(1.00m, summary.HourlyCost);
    Assert.Single(summary.PodBreakdowns);
    Assert.Equal(runningPodId, summary.PodBreakdowns[0].PodId);
  }

  [Fact]
  public async Task Should_Calculate_AutoShutdown_Savings_From_Lifecycle_Events()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodsAsync(
      dbContext,
      organizationId,
      providerId,
      podId,
      podId,
      now,
      hourlyCosts: [2.50m],
      memberCount: 1);

    var pod = await dbContext.GpuPods.SingleAsync(p => p.Id == podId);
    await dbContext.AddPodLifecycleEventAsync(
      new PodLifecycleEvent
      {
        PodId = podId,
        EventType = PodLifecycleEventType.ShutdownCompleted,
        Timestamp = now.AddDays(-5),
        Source = "lifecycle",
        Pod = pod,
      });

    await dbContext.SaveChangesAsync();

    var service = CreateService(dbContext, now);
    var summary = await service.CalculateAsync(organizationId, MetricsPeriod.Daily);

    Assert.Equal(5.00m, summary.AutoShutdownSavings);
  }

  [Fact]
  public async Task Should_Persist_Cost_Snapshot_When_CalculateAndPersistAsync()
  {
    var organizationId = Guid.NewGuid();
    var providerId = Guid.NewGuid();
    var podId = Guid.NewGuid();
    var now = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);

    await using var dbContext = CreateDbContext();
    await SeedProviderAndPodsAsync(
      dbContext,
      organizationId,
      providerId,
      podId,
      podId,
      now,
      hourlyCosts: [1.25m],
      memberCount: 1);

    var notificationService = new Mock<IObservabilityNotificationService>();
    var service = CreateService(dbContext, now, notificationService.Object);
    var summary = await service.CalculateAndPersistAsync(organizationId, MetricsPeriod.Hourly);

    var snapshots = await dbContext.CostSnapshots
      .Where(s => s.OrganizationId == organizationId)
      .ToListAsync();

    Assert.Equal(1.25m, summary.HourlyCost);
    Assert.Single(snapshots);
    Assert.Equal(summary.HourlyCost, snapshots[0].HourlyCost);
    Assert.Equal(summary.DailyCost, snapshots[0].DailyCost);
    notificationService.Verify(
      n => n.NotifyCostUpdatedAsync(organizationId, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  private static CostCalculatorService CreateService(
    ApplicationDbContext dbContext,
    DateTime utcNow,
    IObservabilityNotificationService? notificationService = null) =>
    new(
      dbContext,
      new FixedDateTimeService(utcNow),
      notificationService ?? new NoOpObservabilityNotificationService(),
      NullLogger<CostCalculatorService>.Instance);

  private static ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase($"cost-calculator-{Guid.NewGuid()}")
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

  private static async Task SeedProviderAndPodsAsync(
    ApplicationDbContext dbContext,
    Guid organizationId,
    Guid providerId,
    Guid podOneId,
    Guid podTwoId,
    DateTime now,
    decimal[] hourlyCosts,
    int memberCount = 2)
  {
    await SeedProviderAsync(dbContext, organizationId, providerId, now);

    var podIds = memberCount == 1 ? [podOneId] : new[] { podOneId, podTwoId };
    for (var i = 0; i < podIds.Length; i++)
    {
      await dbContext.AddGpuPodAsync(
        new GpuPod
        {
          Id = podIds[i],
          OrganizationId = organizationId,
          ProviderId = providerId,
          Name = $"pod-{i + 1}",
          Endpoint = $"http://127.0.0.1:{11434 + i}",
          Status = PodStatus.Running,
          GpuId = "gpu-1",
          Region = "US",
          HourlyCost = hourlyCosts[i],
          CreatedAt = now,
        });
    }

    await dbContext.SaveChangesAsync();
  }

  private sealed class FixedDateTimeService(DateTime utcNow) : IDateTimeService
  {
    public DateTime UtcNow { get; } = utcNow;
  }
}
