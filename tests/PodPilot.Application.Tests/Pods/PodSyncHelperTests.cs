using PodPilot.Application.Common;
using PodPilot.Application.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Tests.Pods;

public class PodSyncHelperTests
{
    [Fact]
    public void IsStale_ReturnsTrue_WhenPodHasNeverBeenSynced()
    {
        var pod = new GpuPod
        {
            ProviderPodId = "pod-123",
            Status = PodStatus.Running,
            LastSyncedAt = null,
        };

        Assert.True(PodSyncHelper.IsStale(pod, DateTime.UtcNow));
    }

    [Fact]
    public void IsStale_ReturnsFalse_WhenPodWasRecentlySynced()
    {
        var now = DateTime.UtcNow;
        var pod = new GpuPod
        {
            ProviderPodId = "pod-123",
            Status = PodStatus.Running,
            LastSyncedAt = now.AddSeconds(-10),
        };

        Assert.False(PodSyncHelper.IsStale(pod, now));
    }

    [Fact]
    public void IsVisible_ReturnsFalse_ForDeletedAndDeletingStatuses()
    {
        Assert.False(PodSyncHelper.IsVisible(PodStatus.Deleted));
        Assert.False(PodSyncHelper.IsVisible(PodStatus.Deleting));
        Assert.True(PodSyncHelper.IsVisible(PodStatus.Running));
    }
}
