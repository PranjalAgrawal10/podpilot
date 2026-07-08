namespace PodPilot.Contracts.Pods;

/// <summary>
/// Pod configuration response.
/// </summary>
public sealed class PodConfigurationResponse
{
    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    public string? TemplateId { get; init; }

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// Gets or sets the image name.
    /// </summary>
    public string ImageName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the container disk size in gigabytes.
    /// </summary>
    public int ContainerDiskGb { get; init; }

    /// <summary>
    /// Gets or sets the volume disk size in gigabytes.
    /// </summary>
    public int VolumeDiskGb { get; init; }

    /// <summary>
    /// Gets or sets the volume mount path.
    /// </summary>
    public string VolumeMountPath { get; init; } = "/workspace";

    /// <summary>
    /// Gets or sets the GPU count.
    /// </summary>
    public int GpuCount { get; init; }

    /// <summary>
    /// Gets or sets environment variables.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets exposed ports.
    /// </summary>
    public IReadOnlyList<string> Ports { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether public IP is enabled.
    /// </summary>
    public bool EnablePublicIp { get; init; }
}
