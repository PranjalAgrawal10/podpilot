using PodPilot.Application.Models.Orchestration;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Selects pods from a pool using configurable load balancing strategies.
/// </summary>
public interface ILoadBalancer
{
    /// <summary>
    /// Selects the best pod from a pool for the given request context.
    /// </summary>
    Task<LoadBalancerSelection?> SelectPodAsync(
        LoadBalancerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets load balancer configuration for an organization.
    /// </summary>
    Task<LoadBalancerConfigDto> GetConfigAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates load balancer configuration for an organization.
    /// </summary>
    Task<LoadBalancerConfigDto> UpdateConfigAsync(
        Guid organizationId,
        LoadBalancerConfigDto config,
        CancellationToken cancellationToken = default);
}
