using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Invitations;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Invitations.Commands.InviteMember;

/// <summary>
/// Handles inviting a user to an organization.
/// </summary>
public sealed class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, InvitationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InviteMemberCommandHandler"/> class.
    /// </summary>
    public InviteMemberCommandHandler(
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
    public async Task<InvitationResponse> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var actorId = currentUserService.UserId.Value;
        await organizationAuthorizationService.EnsurePermissionAsync(
            request.OrganizationId,
            actorId,
            PermissionNames.InvitationCreate,
            cancellationToken);

        var role = ApplicationConstants.ParseRoleName(request.Role);
        if (role == OrganizationRole.Owner)
        {
            throw new ForbiddenException("The Owner role cannot be assigned through an invitation.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await identityService.GetUserByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            var existingMembership = await dbContext.OrganizationMembers
                .FirstOrDefaultAsync(
                    m => m.OrganizationId == request.OrganizationId
                         && m.UserId == existingUser.Id
                         && m.IsActive
                         && m.Status == MemberStatus.Active,
                    cancellationToken);

            if (existingMembership is not null)
            {
                throw new ValidationException("User is already a member of this organization.");
            }
        }

        var pendingInvitation = await dbContext.Invitations
            .FirstOrDefaultAsync(
                i => i.OrganizationId == request.OrganizationId
                     && i.Email == normalizedEmail
                     && i.Status == InvitationStatus.Pending
                     && i.ExpiresAt > dateTimeService.UtcNow,
                cancellationToken);

        if (pendingInvitation is not null)
        {
            throw new ValidationException("A pending invitation already exists for this email.");
        }

        var now = dateTimeService.UtcNow;
        var invitation = new Invitation
        {
            OrganizationId = request.OrganizationId,
            Email = normalizedEmail,
            Token = GenerateToken(),
            ExpiresAt = now.AddDays(ApplicationConstants.InvitationExpirationDays),
            Status = InvitationStatus.Pending,
            Role = role,
            CreatedAt = now,
            CreatedBy = actorId.ToString(),
        };

        await dbContext.AddInvitationAsync(invitation, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(Invitation),
            invitation.Id.ToString(),
            $"Invited '{normalizedEmail}' with role '{ApplicationConstants.ToRoleName(role)}'",
            actorId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new InvitationResponse
        {
            Id = invitation.Id,
            OrganizationId = invitation.OrganizationId,
            Email = invitation.Email,
            Role = ApplicationConstants.ToRoleName(invitation.Role),
            Status = invitation.Status.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            Token = invitation.Token,
        };
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
