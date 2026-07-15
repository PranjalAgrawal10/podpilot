using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Application.Routing;

/// <summary>
/// Shared helpers for intelligent routing authorization.
/// </summary>
internal static class RoutingAccess
{
    /// <summary>
    /// Ensures the current user is authenticated and has an organization context.
    /// </summary>
    public static (Guid UserId, Guid OrganizationId) RequireOrganizationContext(ICurrentUserService currentUserService)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        if (!currentUserService.OrganizationId.HasValue)
        {
            throw new ForbiddenException("No organization context is selected.");
        }

        return (currentUserService.UserId.Value, currentUserService.OrganizationId.Value);
    }

    /// <summary>
    /// Ensures the user has the specified permission in the current organization.
    /// </summary>
    public static Task EnsurePermissionAsync(
        IOrganizationAuthorizationService authorizationService,
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken) =>
        authorizationService.EnsurePermissionAsync(
            organizationId,
            userId,
            permission,
            cancellationToken);
}
