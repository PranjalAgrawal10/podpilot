using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Organizations;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Organizations.Commands.UpdateOrganization;

/// <summary>
/// Handles organization updates.
/// </summary>
public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, OrganizationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateOrganizationCommandHandler"/> class.
    /// </summary>
    public UpdateOrganizationCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<OrganizationResponse> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        await organizationAuthorizationService.EnsurePermissionAsync(
            request.OrganizationId,
            userId,
            PermissionNames.OrganizationUpdate,
            cancellationToken);

        var membership = await organizationAuthorizationService.EnsureMemberAsync(
            request.OrganizationId,
            userId,
            cancellationToken);

        var organization = await dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && o.IsActive, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("Organization", request.OrganizationId);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            organization.Name = request.Name.Trim();
            organization.Slug = await CreateUniqueSlugAsync(organization.Name, organization.Id, cancellationToken);
        }

        if (request.Description is not null)
        {
            organization.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        }

        if (request.Logo is not null)
        {
            organization.Logo = string.IsNullOrWhiteSpace(request.Logo) ? null : request.Logo.Trim();
        }

        organization.UpdatedAt = dateTimeService.UtcNow;
        organization.UpdatedBy = userId.ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(Organization),
            organization.Id.ToString(),
            $"Organization '{organization.Name}' updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Slug = organization.Slug,
            Description = organization.Description,
            Logo = organization.Logo,
            OwnerUserId = organization.OwnerUserId,
            IsDefault = organization.IsDefault,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            CurrentUserRole = ApplicationConstants.ToRoleName(membership.Role),
        };
    }

    private async Task<string> CreateUniqueSlugAsync(
        string name,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var baseSlug = ApplicationConstants.CreateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await dbContext.Organizations.AnyAsync(
                   o => o.Slug == slug && o.Id != organizationId,
                   cancellationToken))
        {
            slug = $"{baseSlug}-{++counter}";
        }

        return slug;
    }
}
