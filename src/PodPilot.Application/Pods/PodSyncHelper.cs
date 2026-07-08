using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods;

/// <summary>
/// Shared helpers for synchronizing GPU pods with compute providers.
/// </summary>
internal static class PodSyncHelper
{
    /// <summary>
    /// Synchronizes a pod with the provider and persists updates.
    /// </summary>
    public static async Task SyncWithProviderAsync(
        GpuPod pod,
        Guid organizationId,
        IPodService podService,
        IApplicationDbContext dbContext,
        IPodNotificationService notificationService,
        IDateTimeService dateTimeService,
        string historyMessage,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(pod.ProviderPodId) || pod.Status == PodStatus.Deleted)
        {
            return;
        }

        var previousStatus = pod.Status;
        var info = await podService.SyncPodStatusAsync(pod, cancellationToken);
        var now = dateTimeService.UtcNow;
        pod.UpdatedAt = now;

        if (previousStatus != pod.Status)
        {
            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = now,
                    Message = info.StatusMessage ?? historyMessage,
                },
                cancellationToken);

            await notificationService.NotifyPodStatusChangedAsync(
                organizationId,
                pod.Id,
                pod.Status.ToString(),
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Determines whether a pod should be re-synced from the provider.
    /// </summary>
    public static bool IsStale(GpuPod pod, DateTime utcNow) =>
        !string.IsNullOrWhiteSpace(pod.ProviderPodId)
        && IsVisible(pod.Status)
        && pod.Status != PodStatus.Failed
        && (!pod.LastSyncedAt.HasValue
            || utcNow - pod.LastSyncedAt.Value > TimeSpan.FromSeconds(ApplicationConstants.PodStatusStaleThresholdSeconds));

    /// <summary>
    /// Determines whether a pod should appear in portal listings.
    /// </summary>
    public static bool IsVisible(PodStatus status) =>
        status is not PodStatus.Deleted and not PodStatus.Deleting;
}
