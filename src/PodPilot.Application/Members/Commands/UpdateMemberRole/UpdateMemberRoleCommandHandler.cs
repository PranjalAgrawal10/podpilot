using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Members;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Members.Commands.UpdateMemberRole;

/// <summary>
/// Handles updating a member's organization role.
/// </summary>
public sealed class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, MemberResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMemberRoleCommandHandler"/> class.
    /// </summary>
    public UpdateMemberRoleCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IIdentityService identityService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.identityService = identityService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<MemberResponse> Handle(
        UpdateMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var actorId = currentUserService.UserId.Value;
        await organizationAuthorizationService.EnsurePermissionAsync(
            request.OrganizationId,
            actorId,
            PermissionNames.MemberRoleUpdate,
            cancellationToken);

        var actorMembership = await organizationAuthorizationService.EnsureMemberAsync(
            request.OrganizationId,
            actorId,
            cancellationToken);

        var newRole = ApplicationConstants.ParseRoleName(request.Role);
        if (newRole == OrganizationRole.Owner
            && actorMembership.Role != OrganizationRole.Owner)
        {
            throw new ForbiddenException("Only the owner can assign the Owner role.");
        }

        var member = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(
                m => m.Id == request.MemberId
                     && m.OrganizationId == request.OrganizationId
                     && m.IsActive
                     && m.Status == MemberStatus.Active,
                cancellationToken);

        if (member is null)
        {
            throw new NotFoundException("OrganizationMember", request.MemberId);
        }

        var organization = await dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && o.IsActive, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("Organization", request.OrganizationId);
        }

        if (member.Role == OrganizationRole.Owner
            && newRole != OrganizationRole.Owner)
        {
            var ownerCount = await dbContext.OrganizationMembers.CountAsync(
                m => m.OrganizationId == request.OrganizationId
                     && m.IsActive
                     && m.Status == MemberStatus.Active
                     && m.Role == OrganizationRole.Owner,
                cancellationToken);

            if (ownerCount <= 1)
            {
                throw new ForbiddenException("The last owner cannot be demoted.");
            }
        }

        if (newRole == OrganizationRole.Owner && member.Role != OrganizationRole.Owner)
        {
            var currentOwner = await dbContext.OrganizationMembers
                .FirstOrDefaultAsync(
                    m => m.OrganizationId == request.OrganizationId
                         && m.UserId == organization.OwnerUserId
                         && m.IsActive
                         && m.Status == MemberStatus.Active,
                    cancellationToken);

            if (currentOwner is not null && currentOwner.Id != member.Id)
            {
                currentOwner.Role = OrganizationRole.Admin;
                currentOwner.UpdatedAt = dateTimeService.UtcNow;
                currentOwner.UpdatedBy = actorId.ToString();
            }

            organization.OwnerUserId = member.UserId;
            organization.UpdatedAt = dateTimeService.UtcNow;
            organization.UpdatedBy = actorId.ToString();
        }

        member.Role = newRole;
        member.UpdatedAt = dateTimeService.UtcNow;
        member.UpdatedBy = actorId.ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await identityService.GetUserByIdAsync(member.UserId, cancellationToken)
            ?? throw new NotFoundException("User", member.UserId);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(OrganizationMember),
            member.Id.ToString(),
            $"Updated member role to '{ApplicationConstants.ToRoleName(newRole)}'",
            actorId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new MemberResponse
        {
            Id = member.Id,
            UserId = member.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = ApplicationConstants.ToRoleName(member.Role),
            Status = member.Status.ToString(),
            JoinedAt = member.JoinedAt,
        };
    }
}
