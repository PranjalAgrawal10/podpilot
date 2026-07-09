using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.GetLoadBalancerConfig;

/// <summary>
/// Gets load balancer configuration.
/// </summary>
public sealed class GetLoadBalancerConfigQuery : IRequest<LoadBalancerConfigResponse>
{
}
