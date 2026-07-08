using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Members;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Invitations.Commands.AcceptInvitation;

/// <summary>
/// Handles accepting an organization invitation.
/// </summary>
public sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, MemberResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptInvitationCommandHandler"/> class.
    /// </summary>
    public AcceptInvitationCommandHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.identityService = identityService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<MemberResponse> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        var user = await identityService.GetUserByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException();
        }

        var invitation = await dbContext.Invitations
            .FirstOrDefaultAsync(i => i.Token == request.Token.Trim(), cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ValidationException("This invitation is no longer valid.");
        }

        if (invitation.ExpiresAt <= dateTimeService.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            invitation.UpdatedAt = dateTimeService.UtcNow;
            invitation.UpdatedBy = userId.ToString();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new ValidationException("This invitation has expired.");
        }

        if (!string.Equals(invitation.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("This invitation was sent to a different email address.");
        }

        var existingMembership = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(
                m => m.OrganizationId == invitation.OrganizationId && m.UserId == userId,
                cancellationToken);

        var now = dateTimeService.UtcNow;
        OrganizationMember membership;

        if (existingMembership is not null)
        {
            membership = existingMembership;
            membership.Role = invitation.Role;
            membership.Status = MemberStatus.Active;
            membership.IsActive = true;
            membership.JoinedAt = now;
            membership.UpdatedAt = now;
            membership.UpdatedBy = userId.ToString();
        }
        else
        {
            membership = new OrganizationMember
            {
                OrganizationId = invitation.OrganizationId,
                UserId = userId,
                Role = invitation.Role,
                JoinedAt = now,
                Status = MemberStatus.Active,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = userId.ToString(),
            };

            await dbContext.AddOrganizationMemberAsync(membership, cancellationToken);
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = now;
        invitation.UpdatedAt = now;
        invitation.UpdatedBy = userId.ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(Invitation),
            invitation.Id.ToString(),
            $"Invitation accepted for organization '{invitation.OrganizationId}'",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new MemberResponse
        {
            Id = membership.Id,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = ApplicationConstants.ToRoleName(membership.Role),
            Status = membership.Status.ToString(),
            JoinedAt = membership.JoinedAt,
        };
    }
}
