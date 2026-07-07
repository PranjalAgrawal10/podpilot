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
    /// Gets the audit logs set.
    /// </summary>
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Adds an audit log entry.
    /// </summary>
    /// <param name="auditLog">The audit log to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization.
    /// </summary>
    /// <param name="organization">The organization to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization member.
    /// </summary>
    /// <param name="member">The member to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddOrganizationMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
