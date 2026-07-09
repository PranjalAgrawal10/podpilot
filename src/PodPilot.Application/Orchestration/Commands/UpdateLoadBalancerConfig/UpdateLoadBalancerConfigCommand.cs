using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Commands.UpdateLoadBalancerConfig;

/// <summary>
/// Updates load balancer configuration.
/// </summary>
public sealed class UpdateLoadBalancerConfigCommand : IRequest<LoadBalancerConfigResponse>
{
    /// <summary>Gets or sets the strategy.</summary>
    public string Strategy { get; init; } = "LeastBusy";

    /// <summary>Gets or sets a value indicating whether sticky sessions are enabled.</summary>
    public bool StickySessionsEnabled { get; init; }

    /// <summary>Gets or sets sticky session TTL in minutes.</summary>
    public int StickySessionTtlMinutes { get; init; } = 30;
}
