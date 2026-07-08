using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Scheduler.Commands.CancelSchedulerRequest;

/// <summary>
/// Handles <see cref="CancelSchedulerRequestCommand"/>.
/// </summary>
public sealed class CancelSchedulerRequestCommandHandler : IRequestHandler<CancelSchedulerRequestCommand, bool>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IRequestScheduler scheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelSchedulerRequestCommandHandler"/> class.
    /// </summary>
    public CancelSchedulerRequestCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IRequestScheduler scheduler)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.scheduler = scheduler;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(CancelSchedulerRequestCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);
        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayManage,
            cancellationToken);

        return await scheduler.CancelAsync(request.RequestId, organizationId, cancellationToken);
    }
}
