using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// No-op orchestrator notification service for testing environments.
/// </summary>
public sealed class NoOpOrchestratorNotificationService : IOrchestratorNotificationService
{
    /// <inheritdoc />
    public Task NotifyPodAddedAsync(Guid organizationId, Guid poolId, Guid podId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPodRemovedAsync(Guid organizationId, Guid poolId, Guid podId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyScalingStartedAsync(Guid organizationId, Guid poolId, string direction, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyScalingCompletedAsync(Guid organizationId, Guid poolId, bool success, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPodFailedAsync(Guid organizationId, Guid podId, string reason, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyFailoverTriggeredAsync(Guid organizationId, Guid failedPodId, Guid? replacementPodId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPoolUpdatedAsync(Guid organizationId, Guid poolId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
