using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Organization-scoped authorization backed by membership and role permissions.
/// </summary>
public sealed class OrganizationAuthorizationService : IOrganizationAuthorizationService
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationAuthorizationService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OrganizationAuthorizationService(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<OrganizationMember?> GetMembershipAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        dbContext.OrganizationMembers
            .FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId
                     && m.UserId == userId
                     && m.IsActive
                     && m.Status == MemberStatus.Active,
                cancellationToken);

    /// <inheritdoc />
    public async Task<OrganizationMember> EnsureMemberAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(organizationId, userId, cancellationToken);
        if (membership is null)
        {
            throw new ForbiddenException("You are not a member of this organization.");
        }

        return membership;
    }

    /// <inheritdoc />
    public async Task EnsurePermissionAsync(
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (!await HasPermissionAsync(organizationId, userId, permission, cancellationToken))
        {
            throw new ForbiddenException($"You do not have permission to perform '{permission}'.");
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(organizationId, userId, cancellationToken);
        if (membership is null)
        {
            return false;
        }

        return RolePermissionMatrix.HasPermission(membership.Role, permission);
    }

    /// <inheritdoc />
    public async Task<OrganizationMember?> GetDefaultMembershipAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var defaultMembership = await (
            from member in dbContext.OrganizationMembers
            join organization in dbContext.Organizations on member.OrganizationId equals organization.Id
            where member.UserId == userId
                  && member.IsActive
                  && member.Status == MemberStatus.Active
                  && organization.IsActive
                  && organization.IsDefault
            select member).FirstOrDefaultAsync(cancellationToken);

        if (defaultMembership is not null)
        {
            return defaultMembership;
        }

        return await dbContext.OrganizationMembers
            .Where(m => m.UserId == userId && m.IsActive && m.Status == MemberStatus.Active)
            .OrderBy(m => m.JoinedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
