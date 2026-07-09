using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Manages pod pools and membership.
/// </summary>
public interface IPodPoolManager
{
    /// <summary>
    /// Gets the default or model-matched pool for an organization.
    /// </summary>
    Task<PodPool?> ResolvePoolAsync(
        Guid organizationId,
        string? modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets healthy pool members eligible for request routing.
    /// </summary>
    Task<IReadOnlyList<PoolMemberContext>> GetHealthyMembersAsync(
        Guid poolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod to a pool.
    /// </summary>
    Task<PodPoolMember> AddMemberAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        bool isWarmStandby = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a pod from a pool.
    /// </summary>
    Task RemoveMemberAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a member's orchestration state.
    /// </summary>
    Task UpdateMemberStateAsync(
        Guid poolId,
        Guid podId,
        OrchestrationPodState state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a pod as draining.
    /// </summary>
    Task StartDrainingAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a pod has no active streams.
    /// </summary>
    Task<bool> HasActiveStreamsAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default);
}
