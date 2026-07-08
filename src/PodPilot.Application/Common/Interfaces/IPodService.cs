using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates pod operations with provider adapters and persistence.
/// </summary>
public interface IPodService
{
    /// <summary>
    /// Creates a pod on the provider and returns provider pod information.
    /// </summary>
    Task<PodInfo> CreatePodAsync(
        ComputeProvider provider,
        PodCreateOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a pod on the provider.
    /// </summary>
    Task<PodOperationResult> DeletePodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a pod on the provider.
    /// </summary>
    Task<PodOperationResult> StartPodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a pod on the provider.
    /// </summary>
    Task<PodOperationResult> StopPodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restarts a pod on the provider.
    /// </summary>
    Task<PodOperationResult> RestartPodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports provider pods that are not yet tracked in the database.
    /// </summary>
    Task ImportProviderPodsAsync(
        ComputeProvider provider,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a pod's status from the provider and updates persistence.
    /// </summary>
    Task<PodInfo> SyncPodStatusAsync(
        GpuPod pod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies provider pod information to a persisted GPU pod entity.
    /// </summary>
    void ApplyProviderInfo(GpuPod pod, PodInfo info, DateTime syncedAt);

    /// <summary>
    /// Applies provider status fields without replacing endpoint collections.
    /// </summary>
    void ApplyProviderStatus(GpuPod pod, PodInfo info, DateTime syncedAt);
}
