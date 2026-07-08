using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Lifecycle.Queries.GetPodActivity;

/// <summary>
/// Handles get pod activity queries.
/// </summary>
public sealed class GetPodActivityQueryHandler : IRequestHandler<GetPodActivityQuery, IReadOnlyList<PodActivityResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPodActivityQueryHandler"/> class.
    /// </summary>
    public GetPodActivityQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PodActivityResponse>> Handle(
        GetPodActivityQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodRead,
            cancellationToken);

        await PodAccess.GetPodAsync(dbContext, request.PodId, organizationId, cancellationToken);

        var activities = await dbContext.PodActivities
            .Where(a => a.PodId == request.PodId)
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync(cancellationToken);

        return activities.Select(LifecycleMapper.ToResponse).ToList();
    }
}
