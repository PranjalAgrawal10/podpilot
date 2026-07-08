using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Pods;

/// <summary>
/// Options for creating a pod on a compute provider.
/// </summary>
public sealed class PodCreateOptions
{
    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider GPU identifier.
    /// </summary>
    public string GpuId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU type.
    /// </summary>
    public GpuType GpuType { get; init; }

    /// <summary>
    /// Gets or sets the region identifier.
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional template identifier.
    /// </summary>
    public string? TemplateId { get; init; }

    /// <summary>
    /// Gets or sets the optional template name.
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// Gets or sets the container image name.
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
    /// Gets or sets the number of GPUs.
    /// </summary>
    public int GpuCount { get; init; } = 1;

    /// <summary>
    /// Gets or sets environment variables.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets exposed ports in provider format (e.g. 8888/http).
    /// </summary>
    public IReadOnlyList<string> Ports { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether public endpoints are enabled.
    /// </summary>
    public bool EnablePublicIp { get; init; }
}
