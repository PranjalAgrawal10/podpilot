using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Scheduler;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Scheduler.Queries.ListSchedulerRequests;

/// <summary>
/// Handles <see cref="ListSchedulerRequestsQuery"/>.
/// </summary>
public sealed class ListSchedulerRequestsQueryHandler : IRequestHandler<ListSchedulerRequestsQuery, IReadOnlyList<SchedulerRequestResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListSchedulerRequestsQueryHandler"/> class.
    /// </summary>
    public ListSchedulerRequestsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchedulerRequestResponse>> Handle(
        ListSchedulerRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);
        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var query = dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<GatewayRequestStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(Math.Clamp(request.Limit, 1, 200))
            .ToListAsync(cancellationToken);

        return items.Select(SchedulerMapper.ToResponse).ToList();
    }
}
