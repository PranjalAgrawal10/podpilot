namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Pod pool response.
/// </summary>
public sealed class PodPoolResponse
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the pool name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the pool type.</summary>
    public string PoolType { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this is the default pool.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets a value indicating whether the pool is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the provider identifier.</summary>
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

    /// <summary>Gets or sets the scaling policy identifier.</summary>
    public Guid? ScalingPolicyId { get; set; }

    /// <summary>Gets or sets the scaling policy.</summary>
    public ScalingPolicyResponse? ScalingPolicy { get; set; }

    /// <summary>Gets or sets model names.</summary>
    public IReadOnlyList<string> Models { get; set; } = [];

    /// <summary>Gets or sets pool members.</summary>
    public IReadOnlyList<PodPoolMemberResponse> Members { get; set; } = [];

    /// <summary>Gets or sets when the pool was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets when the pool was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }
}
