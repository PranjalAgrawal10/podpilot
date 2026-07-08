namespace PodPilot.Domain.Entities;

/// <summary>
/// Maps a model name to a GPU pod within an organization.
/// </summary>
public class GatewayRoute : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the model name to route.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default route.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets the target pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;
}
