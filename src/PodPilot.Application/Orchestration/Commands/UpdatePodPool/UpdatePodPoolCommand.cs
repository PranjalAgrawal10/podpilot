using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Commands.UpdatePodPool;

/// <summary>
/// Updates a pod pool.
/// </summary>
public sealed class UpdatePodPoolCommand : IRequest<PodPoolResponse>
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }

    /// <summary>Gets or sets the pool name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the pool type.</summary>
    public string? PoolType { get; init; }

    /// <summary>Gets or sets a value indicating whether this is the default pool.</summary>
    public bool? IsDefault { get; init; }

    /// <summary>Gets or sets a value indicating whether the pool is active.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Gets or sets model names.</summary>
    public IReadOnlyList<string>? Models { get; init; }

    /// <summary>Gets or sets pod identifiers.</summary>
    public IReadOnlyList<Guid>? PodIds { get; init; }

    /// <summary>Gets or sets the scaling policy.</summary>
    public ScalingPolicyRequest? ScalingPolicy { get; init; }
}
