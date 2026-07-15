using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Application.Plugins;

/// <summary>
/// Shared helpers for plugin/MCP authorization.
/// </summary>
internal static class PluginAccess
{
    /// <summary>
    /// Ensures authenticated organization context.
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
    /// Ensures the user has a permission.
    /// </summary>
    public static Task EnsurePermissionAsync(
        IOrganizationAuthorizationService authorizationService,
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken) =>
        authorizationService.EnsurePermissionAsync(organizationId, userId, permission, cancellationToken);
}
