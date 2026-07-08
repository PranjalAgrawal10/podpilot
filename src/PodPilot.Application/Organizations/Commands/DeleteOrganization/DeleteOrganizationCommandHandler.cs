using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Organizations.Commands.DeleteOrganization;

/// <summary>
/// Handles organization deletion.
/// </summary>
public sealed class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOrganizationCommandHandler"/> class.
    /// </summary>
    public DeleteOrganizationCommandHandler(
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
    public async Task<Unit> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        await organizationAuthorizationService.EnsurePermissionAsync(
            request.OrganizationId,
            userId,
            PermissionNames.OrganizationDelete,
            cancellationToken);

        var organization = await dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && o.IsActive, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("Organization", request.OrganizationId);
        }

        if (organization.IsDefault)
        {
            throw new ForbiddenException("The default organization cannot be deleted.");
        }

        await dbContext.Organizations
            .Where(o => o.Id == request.OrganizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(Organization),
            request.OrganizationId.ToString(),
            "Organization deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return Unit.Value;
    }
}
