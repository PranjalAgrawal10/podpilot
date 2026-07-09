namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Request to create a pod pool.
/// </summary>
public sealed class CreatePodPoolRequest
{
    /// <summary>Gets or sets the pool name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the pool type.</summary>
    public string PoolType { get; set; } = "Custom";

    /// <summary>Gets or sets a value indicating whether this is the default pool.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets the provider identifier for auto-provisioning.</summary>
    public Guid? ProviderId { get; set; }

    /// <summary>Gets or sets the GPU identifier.</summary>
    public string? GpuId { get; set; }

    /// <summary>Gets or sets the GPU type.</summary>
    public string? GpuType { get; set; }

    /// <summary>Gets or sets the region.</summary>
    public string? Region { get; set; }

    /// <summary>Gets or sets the template identifier.</summary>
    public string? TemplateId { get; set; }

    /// <summary>Gets or sets the image name.</summary>
    public string? ImageName { get; set; }

    /// <summary>Gets or sets model names served by this pool.</summary>
    public IReadOnlyList<string>? Models { get; set; }

    /// <summary>Gets or sets GPU pod identifiers to add as members.</summary>
    public IReadOnlyList<Guid>? PodIds { get; set; }

    /// <summary>Gets or sets the scaling policy.</summary>
    public ScalingPolicyRequest? ScalingPolicy { get; set; }
}
