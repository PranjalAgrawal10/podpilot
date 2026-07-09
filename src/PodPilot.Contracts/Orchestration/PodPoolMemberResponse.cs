namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Pod pool member response.
/// </summary>
public sealed class PodPoolMemberResponse
{
    /// <summary>Gets or sets the member identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the GPU pod identifier.</summary>
    public Guid GpuPodId { get; set; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; set; } = string.Empty;

    /// <summary>Gets or sets the pod status.</summary>
    public string PodStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets the orchestration state.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the weight.</summary>
    public int Weight { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a warm standby.</summary>
    public bool IsWarmStandby { get; set; }

    /// <summary>Gets or sets active streams.</summary>
    public int ActiveStreams { get; set; }

    /// <summary>Gets or sets when the pod joined.</summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime? LastHealthCheckAt { get; set; }
}
