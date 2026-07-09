using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.GetCapacity;

/// <summary>
/// Handles getting capacity data.
/// </summary>
public sealed class GetCapacityQueryHandler : IRequestHandler<GetCapacityQuery, CapacityResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly ICapacityPlanner capacityPlanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCapacityQueryHandler"/> class.
    /// </summary>
    public GetCapacityQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        ICapacityPlanner capacityPlanner)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.capacityPlanner = capacityPlanner;
    }

    /// <inheritdoc />
    public async Task<CapacityResponse> Handle(GetCapacityQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var plan = await capacityPlanner.CalculateAsync(organizationId, request.PoolId, cancellationToken);
        return OrchestrationMapper.ToCapacityResponse(plan);
    }
}
