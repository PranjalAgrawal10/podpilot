using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Organizations;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Organizations.Queries.ListOrganizations;

/// <summary>
/// Handles listing organizations for the current user.
/// </summary>
public sealed class ListOrganizationsQueryHandler : IRequestHandler<ListOrganizationsQuery, IReadOnlyList<OrganizationResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListOrganizationsQueryHandler"/> class.
    /// </summary>
    public ListOrganizationsQueryHandler(
        ICurrentUserService currentUserService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrganizationResponse>> Handle(
        ListOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;

        return await (
            from member in dbContext.OrganizationMembers
            join organization in dbContext.Organizations on member.OrganizationId equals organization.Id
            where member.UserId == userId
                  && member.IsActive
                  && member.Status == MemberStatus.Active
                  && organization.IsActive
            orderby organization.IsDefault descending, organization.Name
            select new OrganizationResponse
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
                CurrentUserRole = ApplicationConstants.ToRoleName(member.Role),
            }).ToListAsync(cancellationToken);
    }
}
