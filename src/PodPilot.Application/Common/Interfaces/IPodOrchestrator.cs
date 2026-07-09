using PodPilot.Application.Models.Orchestration;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Coordinates multi-pod orchestration, failover, and routing.
/// </summary>
public interface IPodOrchestrator
{
    /// <summary>
    /// Resolves the best pod for a request via load balancer and pool selection.
    /// </summary>
    Task<OrchestratorRouteResult?> ResolvePodAsync(
        OrchestratorRouteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles failover when a pod fails during request processing.
    /// </summary>
    Task<FailoverResult> HandleFailoverAsync(
        Guid organizationId,
        Guid failedPodId,
        Guid? requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orchestrator status for an organization.
    /// </summary>
    Task<OrchestratorStatus> GetStatusAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
