using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Routing.Queries.ListRoutingHistory;

/// <summary>Lists routing decision history.</summary>
public sealed class ListRoutingHistoryQuery : IRequest<IReadOnlyList<RoutingHistoryItemResponse>>
{
    /// <summary>Gets or sets the maximum number of items to return.</summary>
    public int Take { get; init; } = 50;
}

/// <summary>Handles <see cref="ListRoutingHistoryQuery"/>.</summary>
public sealed class ListRoutingHistoryQueryHandler : IRequestHandler<ListRoutingHistoryQuery, IReadOnlyList<RoutingHistoryItemResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>Initializes a new instance of the <see cref="ListRoutingHistoryQueryHandler"/> class.</summary>
    public ListRoutingHistoryQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoutingHistoryItemResponse>> Handle(
        ListRoutingHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = RoutingAccess.RequireOrganizationContext(currentUserService);
        await RoutingAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.RoutingRead, cancellationToken);

        var take = Math.Clamp(request.Take, 1, 200);
        var events = await dbContext.RoutingEvents
            .AsNoTracking()
            .Include(e => e.SelectedProvider)
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.DecidedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return events.Select(RoutingMapper.ToHistoryItemResponse).ToList();
    }
}
