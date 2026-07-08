using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Lifecycle.Queries.GetPodLifecycleEvents;

/// <summary>
/// Handles get pod lifecycle events queries.
/// </summary>
public sealed class GetPodLifecycleEventsQueryHandler
    : IRequestHandler<GetPodLifecycleEventsQuery, IReadOnlyList<PodLifecycleEventResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPodLifecycleEventsQueryHandler"/> class.
    /// </summary>
    public GetPodLifecycleEventsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PodLifecycleEventResponse>> Handle(
        GetPodLifecycleEventsQuery request,
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

        var events = await dbContext.PodLifecycleEvents
            .Where(e => e.PodId == request.PodId)
            .OrderByDescending(e => e.Timestamp)
            .Take(100)
            .ToListAsync(cancellationToken);

        return events.Select(LifecycleMapper.ToResponse).ToList();
    }
}
