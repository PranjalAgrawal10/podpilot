using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Members.Commands.RemoveMember;

/// <summary>
/// Handles removing a member from an organization.
/// </summary>
public sealed class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveMemberCommandHandler"/> class.
    /// </summary>
    public RemoveMemberCommandHandler(
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
    public async Task<Unit> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var actorId = currentUserService.UserId.Value;
        await organizationAuthorizationService.EnsurePermissionAsync(
            request.OrganizationId,
            actorId,
            PermissionNames.MemberManage,
            cancellationToken);

        var member = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(
                m => m.Id == request.MemberId
                     && m.OrganizationId == request.OrganizationId
                     && m.IsActive,
                cancellationToken);

        if (member is null)
        {
            throw new NotFoundException("OrganizationMember", request.MemberId);
        }

        if (member.Role == OrganizationRole.Owner)
        {
            var ownerCount = await dbContext.OrganizationMembers.CountAsync(
                m => m.OrganizationId == request.OrganizationId
                     && m.IsActive
                     && m.Status == MemberStatus.Active
                     && m.Role == OrganizationRole.Owner,
                cancellationToken);

            if (ownerCount <= 1)
            {
                throw new ForbiddenException("The last owner cannot be removed from the organization.");
            }
        }

        member.IsActive = false;
        member.Status = MemberStatus.Suspended;
        member.UpdatedAt = dateTimeService.UtcNow;
        member.UpdatedBy = actorId.ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(OrganizationMember),
            member.Id.ToString(),
            "Member removed from organization",
            actorId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return Unit.Value;
    }
}
