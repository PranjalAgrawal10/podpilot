using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Gateway;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Queries.ListGatewayRoutes;

/// <summary>
/// Handles listing gateway routes.
/// </summary>
public sealed class ListGatewayRoutesQueryHandler : IRequestHandler<ListGatewayRoutesQuery, IReadOnlyList<GatewayRouteResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListGatewayRoutesQueryHandler"/> class.
    /// </summary>
    public ListGatewayRoutesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GatewayRouteResponse>> Handle(
        ListGatewayRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var routes = await dbContext.GatewayRoutes
            .Where(r => r.OrganizationId == organizationId)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.ModelName)
            .ToListAsync(cancellationToken);

        var podIds = routes.Select(r => r.GpuPodId).Distinct().ToList();
        var pods = await dbContext.GpuPods
            .Where(p => podIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        return routes
            .Select(route => GatewayMapper.ToRouteResponse(
                route,
                pods.TryGetValue(route.GpuPodId, out var podName) ? podName : "Unknown"))
            .ToList();
    }
}
