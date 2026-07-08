using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates automatic GPU pod wake and shutdown lifecycle operations.
/// </summary>
public interface IPodLifecycleService
{
    /// <summary>
    /// Records activity for a pod and updates last activity timestamp.
    /// </summary>
    Task RecordActivityAsync(
        Guid podId,
        PodActivityType activityType,
        string source,
        Guid? userId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last activity timestamp for a pod.
    /// </summary>
    Task UpdateLastActivityAsync(
        Guid podId,
        DateTime timestamp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues or executes a wake operation for a stopped pod.
    /// </summary>
    Task<Models.Lifecycle.PodWakeResult> WakePodAsync(
        Guid podId,
        Guid organizationId,
        string source,
        Guid? userId = null,
        bool processImmediately = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a pending wake request.
    /// </summary>
    Task<Models.Lifecycle.PodWakeResult> ProcessWakeRequestAsync(
        PodWakeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down a running pod.
    /// </summary>
    Task<Models.Lifecycle.PodShutdownResult> ShutdownPodAsync(
        Guid podId,
        Guid organizationId,
        string source,
        string reason,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets running pods eligible for idle detection.
    /// </summary>
    Task<IReadOnlyList<GpuPod>> GetRunningPodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pods that exceed idle policy thresholds.
    /// </summary>
    Task<IReadOnlyList<GpuPod>> GetIdlePodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates whether a pod is idle based on its policy.
    /// </summary>
    Models.Lifecycle.PodIdleStatus EvaluateIdleStatus(GpuPod pod, PodIdlePolicy policy, DateTime utcNow);

    /// <summary>
    /// Gets or creates the idle policy for a pod.
    /// </summary>
    Task<PodIdlePolicy> GetOrCreateIdlePolicyAsync(
        Guid podId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to acquire a distributed lifecycle lock.
    /// </summary>
    Task<bool> TryAcquireLockAsync(
        Guid podId,
        PodLifecycleOperation operation,
        string ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a distributed lifecycle lock.
    /// </summary>
    Task ReleaseLockAsync(
        Guid podId,
        PodLifecycleOperation operation,
        string ownerId,
        CancellationToken cancellationToken = default);
}
