using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Orchestration.Commands.DeletePodPool;

/// <summary>
/// Handles pod pool deletion.
/// </summary>
public sealed class DeletePodPoolCommandHandler : IRequestHandler<DeletePodPoolCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IOrchestratorNotificationService notificationService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePodPoolCommandHandler"/> class.
    /// </summary>
    public DeletePodPoolCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IOrchestratorNotificationService notificationService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.notificationService = notificationService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task Handle(DeletePodPoolCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorManage,
            cancellationToken);

        var pool = await OrchestrationAccess.GetPodPoolAsync(
            dbContext,
            request.PoolId,
            organizationId,
            cancellationToken);

        await dbContext.RemovePodPoolAsync(pool.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(PodPool),
            pool.Id.ToString(),
            $"Pod pool '{pool.Name}' deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyPoolUpdatedAsync(organizationId, pool.Id, cancellationToken);
    }
}
