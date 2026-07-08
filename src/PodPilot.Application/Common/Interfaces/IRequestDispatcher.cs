using PodPilot.Application.Models.Scheduler;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Dispatches scheduled requests to GPU pods.
/// </summary>
public interface IRequestDispatcher
{
    /// <summary>
    /// Executes a scheduled request against the assigned pod.
    /// </summary>
    Task<DispatchResult> DispatchAsync(
        DispatchContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects the best available pod for a model within an organization.
    /// </summary>
    Task<PodSelectionResult?> SelectPodAsync(
        Guid organizationId,
        string? modelName,
        Guid? preferredPodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a pod can accept another request.
    /// </summary>
    Task<bool> IsPodAvailableAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves pod capacity for a request.
    /// </summary>
    Task<bool> TryReservePodAsync(
        Guid organizationId,
        Guid podId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases reserved pod capacity.
    /// </summary>
    Task ReleasePodAsync(
        Guid organizationId,
        Guid podId,
        Guid requestId,
        CancellationToken cancellationToken = default);
}
