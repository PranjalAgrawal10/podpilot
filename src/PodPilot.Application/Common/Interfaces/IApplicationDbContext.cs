using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Application database context abstraction.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the refresh tokens set.
    /// </summary>
    IQueryable<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// Gets the organizations set.
    /// </summary>
    IQueryable<Organization> Organizations { get; }

    /// <summary>
    /// Gets the organization members set.
    /// </summary>
    IQueryable<OrganizationMember> OrganizationMembers { get; }

    /// <summary>
    /// Gets the invitations set.
    /// </summary>
    IQueryable<Invitation> Invitations { get; }

    /// <summary>
    /// Gets the permissions set.
    /// </summary>
    IQueryable<Permission> Permissions { get; }

    /// <summary>
    /// Gets the roles set.
    /// </summary>
    IQueryable<Role> Roles { get; }

    /// <summary>
    /// Gets the audit logs set.
    /// </summary>
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Adds an audit log entry.
    /// </summary>
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a refresh token.
    /// </summary>
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization.
    /// </summary>
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization member.
    /// </summary>
    Task AddOrganizationMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an invitation.
    /// </summary>
    Task AddInvitationAsync(Invitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
