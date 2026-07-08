namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an exposed network endpoint for a GPU pod.
/// </summary>
public class PodEndpoint : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the internal port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the protocol (http or tcp).
    /// </summary>
    public string Protocol { get; set; } = "http";

    /// <summary>
    /// Gets or sets the mapped public port.
    /// </summary>
    public int? PublicPort { get; set; }

    /// <summary>
    /// Gets or sets the public URL when available.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets the associated GPU pod.
    /// </summary>
    public GpuPod GpuPod { get; set; } = null!;
}
