using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Gateway;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Gateway.Commands.CreateGatewayRoute;

/// <summary>
/// Handles gateway route creation.
/// </summary>
public sealed class CreateGatewayRouteCommandHandler : IRequestHandler<CreateGatewayRouteCommand, GatewayRouteResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGatewayRouteCommandHandler"/> class.
    /// </summary>
    public CreateGatewayRouteCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<GatewayRouteResponse> Handle(CreateGatewayRouteCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayManage,
            cancellationToken);

        var pod = await PodAccess.GetPodAsync(dbContext, request.GpuPodId, organizationId, cancellationToken);
        if (pod.Status == PodStatus.Deleted)
        {
            throw new Common.Exceptions.NotFoundException("Pod", request.GpuPodId);
        }

        if (request.IsDefault)
        {
            var existingDefaults = await dbContext.GatewayRoutes
                .Where(r => r.OrganizationId == organizationId && r.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var route in existingDefaults)
            {
                route.IsDefault = false;
                route.UpdatedAt = dateTimeService.UtcNow;
            }
        }

        var entity = new GatewayRoute
        {
            OrganizationId = organizationId,
            GpuPodId = request.GpuPodId,
            ModelName = request.ModelName.Trim(),
            IsDefault = request.IsDefault,
            CreatedAt = dateTimeService.UtcNow,
            UpdatedAt = dateTimeService.UtcNow,
        };

        await dbContext.AddGatewayRouteAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return GatewayMapper.ToRouteResponse(entity, pod.Name);
    }
}
