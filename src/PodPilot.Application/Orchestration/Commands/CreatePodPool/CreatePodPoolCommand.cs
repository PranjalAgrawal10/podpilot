using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Commands.CreatePodPool;

/// <summary>
/// Creates a pod pool.
/// </summary>
public sealed class CreatePodPoolCommand : IRequest<PodPoolResponse>
{
    /// <summary>Gets or sets the pool name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the pool type.</summary>
    public string PoolType { get; init; } = "Custom";

    /// <summary>Gets or sets a value indicating whether this is the default pool.</summary>
    public bool IsDefault { get; init; }

    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the GPU identifier.</summary>
    public string? GpuId { get; init; }

    /// <summary>Gets or sets the GPU type.</summary>
    public string? GpuType { get; init; }

    /// <summary>Gets or sets the region.</summary>
    public string? Region { get; init; }

    /// <summary>Gets or sets the template identifier.</summary>
    public string? TemplateId { get; init; }

    /// <summary>Gets or sets the image name.</summary>
    public string? ImageName { get; init; }

    /// <summary>Gets or sets model names.</summary>
    public IReadOnlyList<string>? Models { get; init; }

    /// <summary>Gets or sets pod identifiers to add.</summary>
    public IReadOnlyList<Guid>? PodIds { get; init; }

    /// <summary>Gets or sets the scaling policy.</summary>
    public ScalingPolicyRequest? ScalingPolicy { get; init; }
}
