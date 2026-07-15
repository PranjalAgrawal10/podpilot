using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.DeleteRoutingPolicy;

/// <summary>
/// Handles routing policy deletion.
/// </summary>
public sealed class DeleteRoutingPolicyCommandHandler : IRequestHandler<DeleteRoutingPolicyCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoutingPolicyCommandHandler"/> class.
    /// </summary>
    public DeleteRoutingPolicyCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteRoutingPolicyCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.AiProviderDelete,
            cancellationToken);

        var policy = await AiProviderAccess.GetRoutingPolicyAsync(dbContext, request.PolicyId, organizationId, cancellationToken);
        await dbContext.RemoveAiRoutingPolicyAsync(policy.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(AiRoutingPolicy),
            policy.Id.ToString(),
            $"Routing policy '{policy.Name}' deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return Unit.Value;
    }
}
