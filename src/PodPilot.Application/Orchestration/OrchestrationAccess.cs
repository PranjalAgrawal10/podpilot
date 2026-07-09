using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Orchestration;

/// <summary>
/// Shared helpers for orchestration authorization and lookup.
/// </summary>
internal static class OrchestrationAccess
{
    /// <summary>
    /// Ensures the current user is authenticated and has an organization context.
    /// </summary>
    public static (Guid UserId, Guid OrganizationId) RequireOrganizationContext(ICurrentUserService currentUserService) =>
        PodAccess.RequireOrganizationContext(currentUserService);

    /// <summary>
    /// Ensures the user has the specified permission.
    /// </summary>
    public static Task EnsurePermissionAsync(
        IOrganizationAuthorizationService authorizationService,
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken) =>
        PodAccess.EnsurePermissionAsync(authorizationService, organizationId, userId, permission, cancellationToken);

    /// <summary>
    /// Loads a pod pool scoped to the current organization.
    /// </summary>
    public static async Task<PodPool> GetPodPoolAsync(
        IApplicationDbContext dbContext,
        Guid poolId,
        Guid organizationId,
        CancellationToken cancellationToken,
        bool includeDetails = false)
    {
        var query = dbContext.PodPools.Where(p => p.Id == poolId && p.OrganizationId == organizationId);

        if (includeDetails)
        {
            query = query
                .Include(p => p.Members)
                    .ThenInclude(m => m.GpuPod)
                .Include(p => p.Models)
                .Include(p => p.ScalingPolicy);
        }

        var pool = await query.FirstOrDefaultAsync(cancellationToken);

        if (pool is null)
        {
            throw new NotFoundException("PodPool", poolId);
        }

        return pool;
    }
}
