using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Lifecycle;

/// <summary>
/// Maps lifecycle entities to contract responses.
/// </summary>
internal static class LifecycleMapper
{
    /// <summary>
    /// Maps a pod activity entity to a response.
    /// </summary>
    public static PodActivityResponse ToResponse(PodActivity activity) =>
        new()
        {
            Id = activity.Id,
            ActivityType = activity.ActivityType.ToString(),
            Timestamp = activity.Timestamp,
            Source = activity.Source,
            UserId = activity.UserId,
            Metadata = activity.Metadata,
        };

    /// <summary>
    /// Maps a lifecycle event entity to a response.
    /// </summary>
    public static PodLifecycleEventResponse ToResponse(PodLifecycleEvent lifecycleEvent) =>
        new()
        {
            Id = lifecycleEvent.Id,
            EventType = lifecycleEvent.EventType.ToString(),
            Timestamp = lifecycleEvent.Timestamp,
            Source = lifecycleEvent.Source,
            UserId = lifecycleEvent.UserId,
            Message = lifecycleEvent.Message,
            Metadata = lifecycleEvent.Metadata,
        };

    /// <summary>
    /// Maps an idle policy entity to a response.
    /// </summary>
    public static PodIdlePolicyResponse ToResponse(PodIdlePolicy policy) =>
        new()
        {
            PodId = policy.PodId,
            IdleTimeoutMinutes = policy.IdleTimeoutMinutes,
            GracePeriodMinutes = policy.GracePeriodMinutes,
            AutoShutdownEnabled = policy.AutoShutdownEnabled,
            AutoWakeEnabled = policy.AutoWakeEnabled,
            MinimumRunningTimeMinutes = policy.MinimumRunningTimeMinutes,
            IdleDetectedAt = policy.IdleDetectedAt,
        };
}
