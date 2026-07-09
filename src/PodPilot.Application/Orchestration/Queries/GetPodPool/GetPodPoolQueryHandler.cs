using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.GetPodPool;

/// <summary>
/// Handles getting a pod pool.
/// </summary>
public sealed class GetPodPoolQueryHandler : IRequestHandler<GetPodPoolQuery, PodPoolResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPodPoolQueryHandler"/> class.
    /// </summary>
    public GetPodPoolQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PodPoolResponse> Handle(GetPodPoolQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var pool = await OrchestrationAccess.GetPodPoolAsync(
            dbContext,
            request.PoolId,
            organizationId,
            cancellationToken,
            includeDetails: true);

        return OrchestrationMapper.ToResponse(pool);
    }
}
