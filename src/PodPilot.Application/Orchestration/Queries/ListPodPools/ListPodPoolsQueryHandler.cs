using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.ListPodPools;

/// <summary>
/// Handles listing pod pools.
/// </summary>
public sealed class ListPodPoolsQueryHandler : IRequestHandler<ListPodPoolsQuery, IReadOnlyList<PodPoolResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPodPoolsQueryHandler"/> class.
    /// </summary>
    public ListPodPoolsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PodPoolResponse>> Handle(ListPodPoolsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var pools = await dbContext.PodPools
            .Where(p => p.OrganizationId == organizationId)
            .Include(p => p.Members)
                .ThenInclude(m => m.GpuPod)
            .Include(p => p.Models)
            .Include(p => p.ScalingPolicy)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return pools.Select(OrchestrationMapper.ToResponse).ToList();
    }
}
