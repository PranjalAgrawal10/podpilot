using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Members;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Members.Commands.AddMember;

/// <summary>
/// Handles adding an existing user to an organization.
/// </summary>
public sealed class AddMemberCommandHandler : IRequestHandler<AddMemberCommand, MemberResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddMemberCommandHandler"/> class.
    /// </summary>
    public AddMemberCommandHandler(
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
    public async Task<MemberResponse> Handle(AddMemberCommand request, CancellationToken cancellationToken)
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

        var role = ApplicationConstants.ParseRoleName(request.Role);
        if (role == OrganizationRole.Owner)
        {
            throw new ForbiddenException("Only the owner can assign the Owner role.");
        }

        var user = await identityService.GetUserByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new NotFoundException($"User with email '{request.Email}' was not found.");
        }

        var existingMembership = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(
                m => m.OrganizationId == request.OrganizationId && m.UserId == user.Id,
                cancellationToken);

        if (existingMembership is { IsActive: true, Status: MemberStatus.Active })
        {
            throw new ValidationException("User is already a member of this organization.");
        }

        var now = dateTimeService.UtcNow;

        if (existingMembership is not null)
        {
            existingMembership.Role = role;
            existingMembership.Status = MemberStatus.Active;
            existingMembership.IsActive = true;
            existingMembership.JoinedAt = now;
            existingMembership.UpdatedAt = now;
            existingMembership.UpdatedBy = actorId.ToString();
        }
        else
        {
            existingMembership = new OrganizationMember
            {
                OrganizationId = request.OrganizationId,
                UserId = user.Id,
                Role = role,
                JoinedAt = now,
                Status = MemberStatus.Active,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = actorId.ToString(),
            };

            await dbContext.AddOrganizationMemberAsync(existingMembership, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(OrganizationMember),
            existingMembership.Id.ToString(),
            $"Added member '{user.Email}' with role '{ApplicationConstants.ToRoleName(role)}'",
            actorId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new MemberResponse
        {
            Id = existingMembership.Id,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = ApplicationConstants.ToRoleName(existingMembership.Role),
            Status = existingMembership.Status.ToString(),
            JoinedAt = existingMembership.JoinedAt,
        };
    }
}
