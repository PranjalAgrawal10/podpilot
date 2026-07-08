using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Commands.DeleteGatewayRoute;

/// <summary>
/// Handles gateway route deletion.
/// </summary>
public sealed class DeleteGatewayRouteCommandHandler : IRequestHandler<DeleteGatewayRouteCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteGatewayRouteCommandHandler"/> class.
    /// </summary>
    public DeleteGatewayRouteCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteGatewayRouteCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayManage,
            cancellationToken);

        var route = await dbContext.GatewayRoutes
            .Where(r => r.Id == request.RouteId && r.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Common.Exceptions.NotFoundException("GatewayRoute", request.RouteId);

        await dbContext.RemoveGatewayRouteAsync(route.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
