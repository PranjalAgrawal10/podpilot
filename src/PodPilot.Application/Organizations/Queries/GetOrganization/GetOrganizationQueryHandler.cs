using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Organizations;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Organizations.Queries.GetOrganization;

/// <summary>
/// Handles retrieval of a single organization.
/// </summary>
public sealed class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, OrganizationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrganizationQueryHandler"/> class.
    /// </summary>
    public GetOrganizationQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<OrganizationResponse> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        await organizationAuthorizationService.EnsurePermissionAsync(
            request.OrganizationId,
            userId,
            PermissionNames.OrganizationRead,
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
}
