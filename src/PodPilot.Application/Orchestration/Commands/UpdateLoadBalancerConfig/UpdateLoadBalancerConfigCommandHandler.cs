using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Orchestration.Commands.UpdateLoadBalancerConfig;

/// <summary>
/// Handles updating load balancer configuration.
/// </summary>
public sealed class UpdateLoadBalancerConfigCommandHandler : IRequestHandler<UpdateLoadBalancerConfigCommand, LoadBalancerConfigResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly ILoadBalancer loadBalancer;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLoadBalancerConfigCommandHandler"/> class.
    /// </summary>
    public UpdateLoadBalancerConfigCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        ILoadBalancer loadBalancer,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.loadBalancer = loadBalancer;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<LoadBalancerConfigResponse> Handle(UpdateLoadBalancerConfigCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorManage,
            cancellationToken);

        var config = new LoadBalancerConfigDto
        {
            Strategy = OrchestrationMapper.ParseLoadBalancingStrategy(request.Strategy),
            StickySessionsEnabled = request.StickySessionsEnabled,
            StickySessionTtlMinutes = request.StickySessionTtlMinutes,
        };

        var updated = await loadBalancer.UpdateConfigAsync(organizationId, config, cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(LoadBalancerConfig),
            organizationId.ToString(),
            "Load balancer configuration updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return OrchestrationMapper.ToLoadBalancerConfigResponse(updated);
    }
}
