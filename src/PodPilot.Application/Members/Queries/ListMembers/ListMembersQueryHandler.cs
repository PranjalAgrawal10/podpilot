using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Members;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Members.Queries.ListMembers;

/// <summary>
/// Handles listing organization members.
/// </summary>
public sealed class ListMembersQueryHandler : IRequestHandler<ListMembersQuery, IReadOnlyList<MemberResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListMembersQueryHandler"/> class.
    /// </summary>
    public ListMembersQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IIdentityService identityService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.identityService = identityService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemberResponse>> Handle(
        ListMembersQuery request,
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
            PermissionNames.MemberRead,
            cancellationToken);

        var members = await dbContext.OrganizationMembers
            .Where(m => m.OrganizationId == request.OrganizationId && m.IsActive)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<MemberResponse>(members.Count);
        foreach (var member in members)
        {
            var user = await identityService.GetUserByIdAsync(member.UserId, cancellationToken);
            if (user is null)
            {
                continue;
            }

            responses.Add(new MemberResponse
            {
                Id = member.Id,
                UserId = member.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = ApplicationConstants.ToRoleName(member.Role),
                Status = member.Status.ToString(),
                JoinedAt = member.JoinedAt,
            });
        }

        return responses;
    }
}
