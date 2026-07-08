namespace PodPilot.Contracts.Pods;

/// <summary>
/// Request to create a GPU pod.
/// </summary>
public sealed class CreatePodRequest
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the provider GPU identifier.
    /// </summary>
    public string GpuId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU type.
    /// </summary>
    public string GpuType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region identifier.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional template identifier.
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the optional template name.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Gets or sets the container image name.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the container disk size in gigabytes.
    /// </summary>
    public int ContainerDiskGb { get; set; } = 50;

    /// <summary>
    /// Gets or sets the volume disk size in gigabytes.
    /// </summary>
    public int VolumeDiskGb { get; set; } = 20;

    /// <summary>
    /// Gets or sets the volume mount path.
    /// </summary>
    public string VolumeMountPath { get; set; } = "/workspace";

    /// <summary>
    /// Gets or sets the number of GPUs.
    /// </summary>
    public int GpuCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets environment variables.
    /// </summary>
    public Dictionary<string, string>? EnvironmentVariables { get; set; }

    /// <summary>
    /// Gets or sets exposed ports.
    /// </summary>
    public List<string>? Ports { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether public endpoints are enabled.
    /// </summary>
    public bool EnablePublicIp { get; set; }
}
