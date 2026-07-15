using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Routing.Queries.GetRoutingPolicySettings;

/// <summary>Gets organization intelligent routing policy settings.</summary>
public sealed class GetRoutingPolicySettingsQuery : IRequest<RoutingPolicySettingsResponse>
{
}

/// <summary>Handles <see cref="GetRoutingPolicySettingsQuery"/>.</summary>
public sealed class GetRoutingPolicySettingsQueryHandler : IRequestHandler<GetRoutingPolicySettingsQuery, RoutingPolicySettingsResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IRoutingPolicy routingPolicy;

    /// <summary>Initializes a new instance of the <see cref="GetRoutingPolicySettingsQueryHandler"/> class.</summary>
    public GetRoutingPolicySettingsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IRoutingPolicy routingPolicy)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.routingPolicy = routingPolicy;
    }

    /// <inheritdoc />
    public async Task<RoutingPolicySettingsResponse> Handle(
        GetRoutingPolicySettingsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = RoutingAccess.RequireOrganizationContext(currentUserService);
        await RoutingAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.RoutingRead, cancellationToken);

        var policy = await routingPolicy.GetActivePolicyAsync(organizationId, null, cancellationToken);
        if (policy is null)
        {
            return new RoutingPolicySettingsResponse
            {
                Id = Guid.Empty,
                Name = "Organization Default",
                Strategy = RoutingStrategy.Balanced.ToString(),
                CostWeight = 0.25,
                LatencyWeight = 0.25,
                ReliabilityWeight = 0.20,
                ContextWeight = 0.10,
                FeaturesWeight = 0.10,
                AvailabilityWeight = 0.10,
                MaxRetries = 2,
                FailoverStrategy = AiFailoverStrategy.RetryThenFailover.ToString(),
                IsDefault = true,
            };
        }

        return RoutingMapper.ToPolicySettingsResponse(policy);
    }
}
