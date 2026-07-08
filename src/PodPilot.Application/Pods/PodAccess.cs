using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Pods;

/// <summary>
/// Shared helpers for pod authorization and lookup.
/// </summary>
internal static class PodAccess
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
    /// Loads a pod scoped to the current organization.
    /// </summary>
    public static async Task<GpuPod> GetPodAsync(
        IApplicationDbContext dbContext,
        Guid podId,
        Guid organizationId,
        CancellationToken cancellationToken,
        bool includeDetails = false)
    {
        var query = dbContext.GpuPods
            .Where(p => p.Id == podId && p.OrganizationId == organizationId);

        if (includeDetails)
        {
            query = query
                .Include(p => p.Provider)
                    .ThenInclude(pr => pr.Credential)
                .Include(p => p.Configuration)
                .Include(p => p.Endpoints)
                .Include(p => p.StatusHistory);
        }
        else
        {
            query = query
                .Include(p => p.Provider)
                    .ThenInclude(pr => pr.Credential);
        }

        var pod = await query.FirstOrDefaultAsync(cancellationToken);

        if (pod is null)
        {
            throw new NotFoundException("Pod", podId);
        }

        return pod;
    }

    /// <summary>
    /// Loads a provider scoped to the current organization.
    /// </summary>
    public static async Task<ComputeProvider> GetProviderAsync(
        IApplicationDbContext dbContext,
        Guid providerId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var provider = await dbContext.ComputeProviders
            .Include(p => p.Credential)
            .Where(p => p.Id == providerId && p.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (provider is null)
        {
            throw new NotFoundException("Provider", providerId);
        }

        return provider;
    }

    /// <summary>
    /// Ensures the user has the specified permission in the current organization.
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

    /// <summary>
    /// Serializes environment variables to JSON.
    /// </summary>
    public static string? SerializeEnvironmentVariables(IReadOnlyDictionary<string, string>? variables) =>
        variables is null || variables.Count == 0
            ? null
            : JsonSerializer.Serialize(variables);

    /// <summary>
    /// Deserializes environment variables from JSON.
    /// </summary>
    public static IReadOnlyDictionary<string, string> DeserializeEnvironmentVariables(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Serializes ports to JSON.
    /// </summary>
    public static string? SerializePorts(IReadOnlyList<string>? ports) =>
        ports is null || ports.Count == 0 ? null : JsonSerializer.Serialize(ports);

    /// <summary>
    /// Deserializes ports from JSON.
    /// </summary>
    public static IReadOnlyList<string> DeserializePorts(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }
}
