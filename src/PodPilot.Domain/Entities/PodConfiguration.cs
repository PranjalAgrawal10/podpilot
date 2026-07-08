namespace PodPilot.Domain.Entities;

/// <summary>
/// Stores deployment configuration for a GPU pod.
/// </summary>
public class PodConfiguration : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the template identifier used at creation.
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the template name snapshot.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Gets or sets the container image name.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the container disk size in gigabytes.
    /// </summary>
    public int ContainerDiskGb { get; set; }

    /// <summary>
    /// Gets or sets the volume disk size in gigabytes.
    /// </summary>
    public int VolumeDiskGb { get; set; }

    /// <summary>
    /// Gets or sets the volume mount path.
    /// </summary>
    public string VolumeMountPath { get; set; } = "/workspace";

    /// <summary>
    /// Gets or sets the number of GPUs requested.
    /// </summary>
    public int GpuCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets serialized environment variables as JSON.
    /// </summary>
    public string? EnvironmentVariablesJson { get; set; }

    /// <summary>
    /// Gets or sets serialized exposed ports as JSON.
    /// </summary>
    public string? PortsJson { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether public endpoints are enabled.
    /// </summary>
    public bool EnablePublicIp { get; set; }

    /// <summary>
    /// Gets the associated GPU pod.
    /// </summary>
    public GpuPod GpuPod { get; set; } = null!;
}
