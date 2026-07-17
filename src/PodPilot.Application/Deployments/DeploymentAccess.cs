using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Deployments;

/// <summary>
/// Shared helpers for deployment authorization and lookup.
/// </summary>
internal static class DeploymentAccess
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
    /// Ensures the user has the specified permission.
    /// </summary>
    public static Task EnsurePermissionAsync(
        IOrganizationAuthorizationService authorizationService,
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken) =>
        authorizationService.EnsurePermissionAsync(organizationId, userId, permission, cancellationToken);

    /// <summary>
    /// Loads a deployment scoped to the organization.
    /// </summary>
    public static async Task<AiDeployment> GetDeploymentAsync(
        IApplicationDbContext db,
        Guid deploymentId,
        Guid organizationId,
        CancellationToken cancellationToken,
        bool includeDetails = false)
    {
        var query = db.AiDeployments.Where(d => d.Id == deploymentId && d.OrganizationId == organizationId);

        if (includeDetails)
        {
            query = query
                .Include(d => d.Models)
                .Include(d => d.Logs)
                .Include(d => d.Health)
                .Include(d => d.Provider);
        }

        var deployment = await query.FirstOrDefaultAsync(cancellationToken);
        if (deployment is null)
        {
            throw new NotFoundException("Deployment", deploymentId);
        }

        return deployment;
    }
}
