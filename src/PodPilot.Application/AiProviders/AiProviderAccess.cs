using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.AiProviders;

/// <summary>
/// Shared helpers for AI provider authorization and lookup.
/// </summary>
internal static class AiProviderAccess
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
    /// Loads an AI provider scoped to the current organization.
    /// </summary>
    public static async Task<AiInferenceProvider> GetProviderAsync(
        IApplicationDbContext dbContext,
        Guid providerId,
        Guid organizationId,
        CancellationToken cancellationToken,
        bool includeCredential = false,
        bool includeHealth = false,
        bool includeModels = false)
    {
        var query = dbContext.AiInferenceProviders
            .Where(p => p.Id == providerId && p.OrganizationId == organizationId);

        if (includeCredential)
        {
            query = query.Include(p => p.Credential);
        }

        if (includeHealth)
        {
            query = query.Include(p => p.Health);
        }

        if (includeModels)
        {
            query = query.Include(p => p.Models);
        }

        var provider = await query.FirstOrDefaultAsync(cancellationToken);
        if (provider is null)
        {
            throw new NotFoundException("AI provider", providerId);
        }

        return provider;
    }

    /// <summary>
    /// Loads a routing policy scoped to the current organization.
    /// </summary>
    public static async Task<AiRoutingPolicy> GetRoutingPolicyAsync(
        IApplicationDbContext dbContext,
        Guid policyId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var policy = await dbContext.AiRoutingPolicies
            .Include(p => p.PrimaryProvider)
            .FirstOrDefaultAsync(
                p => p.Id == policyId && p.OrganizationId == organizationId,
                cancellationToken);

        if (policy is null)
        {
            throw new NotFoundException("Routing policy", policyId);
        }

        return policy;
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
