using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models;

/// <summary>
/// Shared helpers for model authorization and lookup.
/// </summary>
internal static class ModelAccess
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
    /// Loads a model scoped to the organization.
    /// </summary>
    public static async Task<AiModel> GetModelAsync(
        IModelRepository repository,
        Guid organizationId,
        Guid modelId,
        CancellationToken cancellationToken)
    {
        var model = await repository.GetByIdAsync(organizationId, modelId, cancellationToken);
        if (model is null)
        {
            throw new NotFoundException("Model", modelId);
        }

        return model;
    }

    /// <summary>
    /// Validates that a pod belongs to the organization and is usable.
    /// </summary>
    public static async Task<GpuPod> GetPodAsync(
        IApplicationDbContext dbContext,
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken)
    {
        var pod = await dbContext.GpuPods
            .Where(p => p.Id == podId && p.OrganizationId == organizationId && p.Status != PodStatus.Deleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (pod is null)
        {
            throw new NotFoundException("Pod", podId);
        }

        return pod;
    }
}
