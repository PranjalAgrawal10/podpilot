namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Request to update a pod pool.
/// </summary>
public sealed class UpdatePodPoolRequest
{
    /// <summary>Gets or sets the pool name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the pool type.</summary>
    public string? PoolType { get; set; }

    /// <summary>Gets or sets a value indicating whether this is the default pool.</summary>
    public bool? IsDefault { get; set; }

    /// <summary>Gets or sets a value indicating whether the pool is active.</summary>
    public bool? IsActive { get; set; }

    /// <summary>Gets or sets model names.</summary>
    public IReadOnlyList<string>? Models { get; set; }

    /// <summary>Gets or sets GPU pod identifiers to set as members.</summary>
    public IReadOnlyList<Guid>? PodIds { get; set; }

    /// <summary>Gets or sets the scaling policy.</summary>
    public ScalingPolicyRequest? ScalingPolicy { get; set; }
}
