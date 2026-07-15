using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.DeleteAiProvider;

/// <summary>
/// Handles AI provider deletion.
/// </summary>
public sealed class DeleteAiProviderCommandHandler : IRequestHandler<DeleteAiProviderCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IAiProviderNotificationService notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAiProviderCommandHandler"/> class.
    /// </summary>
    public DeleteAiProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IAiProviderNotificationService notificationService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteAiProviderCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.AiProviderDelete,
            cancellationToken);

        var provider = await AiProviderAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken);

        await dbContext.RemoveAiInferenceProviderAsync(provider.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(AiInferenceProvider),
            provider.Id.ToString(),
            $"AI provider '{provider.DisplayName}' deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyProviderDisconnectedAsync(organizationId, provider.Id, cancellationToken);
        return Unit.Value;
    }
}
