using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// A pool of GPU pods for load-balanced inference.
/// </summary>
public class PodPool : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the pool name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the pool type.
    /// </summary>
    public PodPoolType PoolType { get; set; } = PodPoolType.Custom;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default pool.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pool is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the provider used for auto-provisioned pods.
    /// </summary>
    public Guid? ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the GPU identifier for auto-provisioned pods.
    /// </summary>
    public string? GpuId { get; set; }

    /// <summary>
    /// Gets or sets the GPU type for auto-provisioned pods.
    /// </summary>
    public GpuType? GpuType { get; set; }

    /// <summary>
    /// Gets or sets the region for auto-provisioned pods.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Gets or sets the template identifier for auto-provisioned pods.
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the image name for auto-provisioned pods.
    /// </summary>
    public string? ImageName { get; set; }

    /// <summary>
    /// Gets or sets the scaling policy identifier.
    /// </summary>
    public Guid? ScalingPolicyId { get; set; }

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets the scaling policy.
    /// </summary>
    public ScalingPolicy? ScalingPolicy { get; set; }

    /// <summary>
    /// Gets pool members.
    /// </summary>
    public ICollection<PodPoolMember> Members { get; set; } = [];

    /// <summary>
    /// Gets models served by this pool.
    /// </summary>
    public ICollection<PodPoolModel> Models { get; set; } = [];
}
