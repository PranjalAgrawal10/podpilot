namespace PodPilot.Contracts.Pods;

/// <summary>
/// GPU pod response.
/// </summary>
public sealed class PodResponse
{
    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; init; }

    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets or sets the provider display name.
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public string ProviderType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider pod identifier.
    /// </summary>
    public string? ProviderPodId { get; init; }

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU type.
    /// </summary>
    public string GpuType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider GPU identifier.
    /// </summary>
    public string GpuId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the region.
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    public string? TemplateId { get; init; }

    /// <summary>
    /// Gets or sets the image name.
    /// </summary>
    public string ImageName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the container disk size in gigabytes.
    /// </summary>
    public int ContainerDisk { get; init; }

    /// <summary>
    /// Gets or sets the volume disk size in gigabytes.
    /// </summary>
    public int VolumeDisk { get; init; }

    /// <summary>
    /// Gets or sets the public IP address.
    /// </summary>
    public string? PublicIp { get; init; }

    /// <summary>
    /// Gets or sets the primary endpoint URL.
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the pod is public.
    /// </summary>
    public bool IsPublic { get; init; }

    /// <summary>
    /// Gets or sets the hourly cost.
    /// </summary>
    public decimal? HourlyCost { get; init; }

    /// <summary>
    /// Gets or sets when the pod was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets when the pod was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Gets or sets when the pod was last started.
    /// </summary>
    public DateTime? LastStartedAt { get; init; }

    /// <summary>
    /// Gets or sets when the pod was last stopped.
    /// </summary>
    public DateTime? LastStoppedAt { get; init; }

    /// <summary>
    /// Gets or sets when status was last synchronized.
    /// </summary>
    public DateTime? LastSyncedAt { get; init; }

    /// <summary>
    /// Gets or sets exposed endpoints.
    /// </summary>
    public IReadOnlyList<PodEndpointResponse> Endpoints { get; init; } = [];

    /// <summary>
    /// Gets or sets recent status history.
    /// </summary>
    public IReadOnlyList<PodStatusHistoryResponse> StatusHistory { get; init; } = [];

    /// <summary>
    /// Gets or sets the pod configuration.
    /// </summary>
    public PodConfigurationResponse? Configuration { get; init; }
}
