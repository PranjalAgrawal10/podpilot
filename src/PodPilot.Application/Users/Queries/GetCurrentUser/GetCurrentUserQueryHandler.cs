using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Users;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Users.Queries.GetCurrentUser;

/// <summary>
/// Handles retrieval of the current authenticated user.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentUserQueryHandler"/> class.
    /// </summary>
    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.identityService = identityService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        var user = await identityService.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = await identityService.GetUserRolesAsync(userId, cancellationToken);

        var organizations = await dbContext.OrganizationMembers
            .Where(m => m.UserId == userId
                        && m.IsActive
                        && m.Status == MemberStatus.Active)
            .Join(
                dbContext.Organizations.Where(o => o.IsActive),
                member => member.OrganizationId,
                organization => organization.Id,
                (member, organization) => new OrganizationSummary
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    Slug = organization.Slug,
                    Role = ApplicationConstants.ToRoleName(member.Role),
                })
            .ToListAsync(cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            Organizations = organizations,
        };
    }
}
