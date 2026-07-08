using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Providers.Commands.DeleteProvider;

/// <summary>
/// Handles compute provider deletion.
/// </summary>
public sealed class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteProviderCommandHandler"/> class.
    /// </summary>
    public DeleteProviderCommandHandler(
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
    public async Task<Unit> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderDelete,
            cancellationToken);

        var provider = await ProviderAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken);

        await dbContext.RemoveComputeProviderAsync(provider.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(ComputeProvider),
            provider.Id.ToString(),
            $"Provider '{provider.DisplayName}' deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return Unit.Value;
    }
}
