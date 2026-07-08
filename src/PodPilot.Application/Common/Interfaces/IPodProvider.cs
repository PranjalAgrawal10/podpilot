using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Vendor-neutral contract for GPU pod lifecycle operations.
/// </summary>
public interface IPodProvider
{
    /// <summary>
    /// Gets the provider type implemented by this adapter.
    /// </summary>
    ProviderType ProviderType { get; }

    /// <summary>
    /// Creates a pod on the provider.
    /// </summary>
    Task<PodInfo> CreatePodAsync(
        string apiKey,
        Models.Pods.PodCreateOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a pod on the provider.
    /// </summary>
    Task<PodOperationResult> DeletePodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a stopped pod.
    /// </summary>
    Task<PodOperationResult> StartPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a running pod.
    /// </summary>
    Task<PodOperationResult> StopPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restarts a pod.
    /// </summary>
    Task<PodOperationResult> RestartPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pod details from the provider.
    /// </summary>
    Task<PodInfo> GetPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pods from the provider.
    /// </summary>
    Task<IReadOnlyList<PodInfo>> ListPodsAsync(
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes pod status from the provider.
    /// </summary>
    Task<PodInfo> SyncPodStatusAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default);
}
