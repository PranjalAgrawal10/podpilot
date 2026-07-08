using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Scheduler;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Scheduler.Queries.GetSchedulerRequest;

/// <summary>
/// Handles <see cref="GetSchedulerRequestQuery"/>.
/// </summary>
public sealed class GetSchedulerRequestQueryHandler : IRequestHandler<GetSchedulerRequestQuery, SchedulerRequestResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerRequestQueryHandler"/> class.
    /// </summary>
    public GetSchedulerRequestQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<SchedulerRequestResponse> Handle(
        GetSchedulerRequestQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);
        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var entity = await dbContext.GatewayRequests
            .Where(r => r.Id == request.RequestId && r.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Request", request.RequestId);

        return SchedulerMapper.ToResponse(entity);
    }
}
