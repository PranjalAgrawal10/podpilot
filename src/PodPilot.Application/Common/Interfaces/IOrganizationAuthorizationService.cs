using PodPilot.Contracts.Auth;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Organization-scoped authorization service.
/// </summary>
public interface IOrganizationAuthorizationService
{
    /// <summary>
    /// Gets the active membership for a user in an organization.
    /// </summary>
    Task<OrganizationMember?> GetMembershipAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the user is an active member of the organization.
    /// </summary>
    Task<OrganizationMember> EnsureMemberAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the user has the specified permission in the organization.
    /// </summary>
    Task EnsurePermissionAsync(
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the user has the specified permission.
    /// </summary>
    Task<bool> HasPermissionAsync(
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's default or first active organization membership.
    /// </summary>
    Task<OrganizationMember?> GetDefaultMembershipAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
