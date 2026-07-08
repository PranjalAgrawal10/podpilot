using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Providers;

/// <summary>
/// Shared helpers for provider authorization and lookup.
/// </summary>
internal static class ProviderAccess
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
    /// Loads a provider scoped to the current organization.
    /// </summary>
    public static async Task<ComputeProvider> GetProviderAsync(
        IApplicationDbContext dbContext,
        Guid providerId,
        Guid organizationId,
        CancellationToken cancellationToken,
        bool includeCredential = false)
    {
        var query = dbContext.ComputeProviders
            .Where(p => p.Id == providerId && p.OrganizationId == organizationId);

        if (includeCredential)
        {
            query = query.Include(p => p.Credential);
        }

        var provider = await query.FirstOrDefaultAsync(cancellationToken);

        if (provider is null)
        {
            throw new NotFoundException("Provider", providerId);
        }

        return provider;
    }

    /// <summary>
    /// Ensures the user has the specified provider permission in the current organization.
    /// </summary>
    public static async Task EnsurePermissionAsync(
        IOrganizationAuthorizationService authorizationService,
        Guid organizationId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(
            organizationId,
            userId,
            permission,
            cancellationToken);
    }
}
