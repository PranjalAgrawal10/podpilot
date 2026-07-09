using PodPilot.Application.Models.Orchestration;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Calculates capacity metrics and scaling recommendations.
/// </summary>
public interface ICapacityPlanner
{
    /// <summary>
    /// Calculates current capacity for an organization or pool.
    /// </summary>
    Task<CapacityPlan> CalculateAsync(
        Guid organizationId,
        Guid? poolId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a capacity snapshot.
    /// </summary>
    Task RecordSnapshotAsync(
        Guid organizationId,
        Guid? poolId = null,
        CancellationToken cancellationToken = default);
}
