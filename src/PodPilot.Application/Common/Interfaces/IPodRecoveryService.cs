using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Replaces a GPU pod on the provider when start operations fail.
/// </summary>
public interface IPodRecoveryService
{
    /// <summary>
    /// Creates a similar provider pod, migrates tracked data, and deletes the failed instance.
    /// </summary>
    Task<PodRecoveryResult> TryReplacePodOnStartFailureAsync(
        GpuPod pod,
        Guid organizationId,
        string source,
        Guid? userId,
        string? failureReason,
        CancellationToken cancellationToken = default);
}
