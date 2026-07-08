using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a GPU pod managed within an organization.
/// </summary>
public class GpuPod : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the compute provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the provider-specific pod identifier.
    /// </summary>
    public string? ProviderPodId { get; set; }

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the current pod status.
    /// </summary>
    public PodStatus Status { get; set; } = PodStatus.Unknown;

    /// <summary>
    /// Gets or sets the GPU type.
    /// </summary>
    public GpuType GpuType { get; set; }

    /// <summary>
    /// Gets or sets the provider GPU identifier.
    /// </summary>
    public string GpuId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region identifier.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the container image name.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the container disk size in gigabytes.
    /// </summary>
    public int ContainerDisk { get; set; }

    /// <summary>
    /// Gets or sets the volume disk size in gigabytes.
    /// </summary>
    public int VolumeDisk { get; set; }

    /// <summary>
    /// Gets or sets the public IP address.
    /// </summary>
    public string? PublicIp { get; set; }

    /// <summary>
    /// Gets or sets the primary endpoint URL.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pod exposes a public endpoint.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets the hourly cost in provider currency.
    /// </summary>
    public decimal? HourlyCost { get; set; }

    /// <summary>
    /// Gets or sets when the pod was last started.
    /// </summary>
    public DateTime? LastStartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the pod was last stopped.
    /// </summary>
    public DateTime? LastStoppedAt { get; set; }

    /// <summary>
    /// Gets or sets when status was last synchronized with the provider.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets the compute provider.
    /// </summary>
    public ComputeProvider Provider { get; set; } = null!;

    /// <summary>
    /// Gets the pod configuration.
    /// </summary>
    public PodConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets exposed endpoints for this pod.
    /// </summary>
    public ICollection<PodEndpoint> Endpoints { get; set; } = [];

    /// <summary>
    /// Gets status history entries.
    /// </summary>
    public ICollection<PodStatusHistory> StatusHistory { get; set; } = [];
}
