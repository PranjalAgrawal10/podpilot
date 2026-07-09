using PodPilot.Application.Models.Orchestration;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Evaluates scaling policies and triggers scale up/down actions.
/// </summary>
public interface IAutoScaler
{
    /// <summary>
    /// Evaluates all pools for an organization and scales if thresholds are exceeded.
    /// </summary>
    Task<IReadOnlyList<ScalingActionResult>> EvaluateAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually scales up a pool by one pod.
    /// </summary>
    Task<ScalingActionResult> ScaleUpAsync(
        Guid organizationId,
        Guid poolId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually scales down a pool by one pod.
    /// </summary>
    Task<ScalingActionResult> ScaleDownAsync(
        Guid organizationId,
        Guid poolId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets auto-scaler status for an organization.
    /// </summary>
    Task<AutoScalerStatus> GetStatusAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
