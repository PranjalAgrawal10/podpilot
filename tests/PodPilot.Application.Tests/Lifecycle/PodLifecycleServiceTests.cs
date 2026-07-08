using PodPilot.Application.Common;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Lifecycle;

public class PodLifecycleServiceTests
{
    [Fact]
    public void EvaluateIdleStatus_ReturnsIdle_WhenTimeoutAndGraceExceeded()
    {
        var service = CreateService();
        var now = new DateTime(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);
        var pod = new GpuPod
        {
            Id = Guid.NewGuid(),
            Status = PodStatus.Running,
            LastActivityAt = now.AddMinutes(-40),
            CreatedAt = now.AddHours(-2),
        };
        var policy = new PodIdlePolicy
        {
            IdleTimeoutMinutes = 30,
            GracePeriodMinutes = 5,
            AutoShutdownEnabled = true,
        };

        var result = service.EvaluateIdleStatus(pod, policy, now);

        Assert.True(result.IsIdle);
        Assert.True(result.IdleMinutes >= 40);
        Assert.NotNull(result.NextShutdownAt);
        Assert.True(result.NextShutdownAt <= now);
    }

    [Fact]
    public void EvaluateIdleStatus_ReturnsNotIdle_WhenRecentlyActive()
    {
        var service = CreateService();
        var now = DateTime.UtcNow;
        var pod = new GpuPod
        {
            Id = Guid.NewGuid(),
            Status = PodStatus.Running,
            LastActivityAt = now.AddMinutes(-5),
            CreatedAt = now.AddHours(-1),
        };
        var policy = new PodIdlePolicy
        {
            IdleTimeoutMinutes = 30,
            GracePeriodMinutes = 5,
        };

        var result = service.EvaluateIdleStatus(pod, policy, now);

        Assert.False(result.IsIdle);
        Assert.Null(result.NextShutdownAt);
    }

    [Fact]
    public void EvaluateIdleStatus_SchedulesShutdown_AfterGracePeriod()
    {
        var service = CreateService();
        var now = new DateTime(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);
        var pod = new GpuPod
        {
            Id = Guid.NewGuid(),
            Status = PodStatus.Running,
            LastActivityAt = now.AddMinutes(-32),
            CreatedAt = now.AddHours(-2),
        };
        var policy = new PodIdlePolicy
        {
            IdleTimeoutMinutes = 30,
            GracePeriodMinutes = 5,
        };

        var result = service.EvaluateIdleStatus(pod, policy, now);

        Assert.True(result.IsIdle);
        Assert.True(result.NextShutdownAt > now);
    }

    private static PodLifecycleService CreateService() =>
        new(
            null!,
            null!,
            null!,
            null!,
            null!);
}
