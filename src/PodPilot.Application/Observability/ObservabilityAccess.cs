using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Observability;

/// <summary>
/// Shared helpers for observability authorization and lookup.
/// </summary>
internal static class ObservabilityAccess
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
    /// Loads a GPU pod scoped to the current organization.
    /// </summary>
    public static async Task<GpuPod> GetGpuPodAsync(
        IApplicationDbContext dbContext,
        Guid podId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var pod = await dbContext.GpuPods
            .Where(p => p.Id == podId && p.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (pod is null)
        {
            throw new NotFoundException("GpuPod", podId);
        }

        return pod;
    }

    /// <summary>
    /// Loads a compute provider scoped to the current organization.
    /// </summary>
    public static async Task<ComputeProvider> GetProviderAsync(
        IApplicationDbContext dbContext,
        Guid providerId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var provider = await dbContext.ComputeProviders
            .Where(p => p.Id == providerId && p.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (provider is null)
        {
            throw new NotFoundException("ComputeProvider", providerId);
        }

        return provider;
    }
}
