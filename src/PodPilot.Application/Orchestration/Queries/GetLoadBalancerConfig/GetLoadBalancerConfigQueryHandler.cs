using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.GetLoadBalancerConfig;

/// <summary>
/// Handles getting load balancer configuration.
/// </summary>
public sealed class GetLoadBalancerConfigQueryHandler : IRequestHandler<GetLoadBalancerConfigQuery, LoadBalancerConfigResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly ILoadBalancer loadBalancer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLoadBalancerConfigQueryHandler"/> class.
    /// </summary>
    public GetLoadBalancerConfigQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        ILoadBalancer loadBalancer)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.loadBalancer = loadBalancer;
    }

    /// <inheritdoc />
    public async Task<LoadBalancerConfigResponse> Handle(GetLoadBalancerConfigQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var config = await loadBalancer.GetConfigAsync(organizationId, cancellationToken);
        return OrchestrationMapper.ToLoadBalancerConfigResponse(config);
    }
}
