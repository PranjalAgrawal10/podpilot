using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.ListScalingEvents;

/// <summary>
/// Handles listing scaling events.
/// </summary>
public sealed class ListScalingEventsQueryHandler : IRequestHandler<ListScalingEventsQuery, IReadOnlyList<ScalingEventResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListScalingEventsQueryHandler"/> class.
    /// </summary>
    public ListScalingEventsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScalingEventResponse>> Handle(ListScalingEventsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var query = dbContext.ScalingEvents.Where(e => e.OrganizationId == organizationId);

        if (request.PoolId.HasValue)
        {
            query = query.Where(e => e.PodPoolId == request.PoolId.Value);
        }

        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return events.Select(OrchestrationMapper.ToScalingEventResponse).ToList();
    }
}
