using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Pods;

/// <summary>
/// Pod information returned by a compute provider.
/// </summary>
public sealed class PodInfo
{
    /// <summary>
    /// Gets or sets the provider pod identifier.
    /// </summary>
    public string ProviderPodId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the pod status.
    /// </summary>
    public PodStatus Status { get; init; }

    /// <summary>
    /// Gets or sets the GPU identifier.
    /// </summary>
    public string? GpuId { get; init; }

    /// <summary>
    /// Gets or sets the GPU type.
    /// </summary>
    public GpuType GpuType { get; init; }

    /// <summary>
    /// Gets or sets the region or data center identifier.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    public string? TemplateId { get; init; }

    /// <summary>
    /// Gets or sets the container image name.
    /// </summary>
    public string? ImageName { get; init; }

    /// <summary>
    /// Gets or sets the container disk size in gigabytes.
    /// </summary>
    public int? ContainerDiskGb { get; init; }

    /// <summary>
    /// Gets or sets the volume disk size in gigabytes.
    /// </summary>
    public int? VolumeDiskGb { get; init; }

    /// <summary>
    /// Gets or sets the public IP address.
    /// </summary>
    public string? PublicIp { get; init; }

    /// <summary>
    /// Gets or sets the primary endpoint URL.
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets or sets exposed endpoints.
    /// </summary>
    public IReadOnlyList<PodEndpointInfo> Endpoints { get; init; } = [];

    /// <summary>
    /// Gets or sets the hourly cost.
    /// </summary>
    public decimal? HourlyCost { get; init; }

    /// <summary>
    /// Gets or sets when the pod was last started.
    /// </summary>
    public DateTime? LastStartedAt { get; init; }

    /// <summary>
    /// Gets or sets when the pod was last stopped.
    /// </summary>
    public DateTime? LastStoppedAt { get; init; }

    /// <summary>
    /// Gets or sets an optional provider status message.
    /// </summary>
    public string? StatusMessage { get; init; }
}
