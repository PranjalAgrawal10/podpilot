namespace PodPilot.Contracts.Providers;

/// <summary>
/// Provider GPU response.
/// </summary>
public sealed class ProviderGpuResponse
{
    /// <summary>
    /// Gets or sets the GPU identifier.
    /// </summary>
    public string GpuId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU type.
    /// </summary>
    public string GpuType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GPU memory in gigabytes.
    /// </summary>
    public int? MemoryGb { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the GPU is available.
    /// </summary>
    public bool IsAvailable { get; init; }
}
