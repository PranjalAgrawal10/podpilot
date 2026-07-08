using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Compute;

/// <summary>
/// GPU information from a compute provider.
/// </summary>
public sealed class ProviderGpuInfo
{
    /// <summary>
    /// Gets or sets the provider-specific GPU identifier.
    /// </summary>
    public string GpuId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapped GPU type.
    /// </summary>
    public GpuType GpuType { get; init; }

    /// <summary>
    /// Gets or sets the GPU memory in gigabytes.
    /// </summary>
    public int? MemoryGb { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the GPU is available.
    /// </summary>
    public bool IsAvailable { get; init; } = true;
}
