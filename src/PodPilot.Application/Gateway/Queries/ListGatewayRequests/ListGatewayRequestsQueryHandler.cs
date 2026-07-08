using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Gateway;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Queries.ListGatewayRequests;

/// <summary>
/// Handles listing recent gateway requests.
/// </summary>
public sealed class ListGatewayRequestsQueryHandler
    : IRequestHandler<ListGatewayRequestsQuery, IReadOnlyList<GatewayRequestSummaryResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListGatewayRequestsQueryHandler"/> class.
    /// </summary>
    public ListGatewayRequestsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GatewayRequestSummaryResponse>> Handle(
        ListGatewayRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var limit = Math.Clamp(request.Limit, 1, 200);
        var requests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId)
            .Include(r => r.Latency)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return requests.Select(GatewayMapper.ToRequestSummary).ToList();
    }
}
