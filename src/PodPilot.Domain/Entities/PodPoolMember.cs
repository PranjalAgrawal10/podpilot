using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Membership of a GPU pod within a pod pool.
/// </summary>
public class PodPoolMember : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the pod pool identifier.
    /// </summary>
    public Guid PodPoolId { get; set; }

    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the orchestration state.
    /// </summary>
    public OrchestrationPodState State { get; set; } = OrchestrationPodState.Provisioning;

    /// <summary>
    /// Gets or sets the load balancing weight.
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether this is a warm standby pod.
    /// </summary>
    public bool IsWarmStandby { get; set; }

    /// <summary>
    /// Gets or sets when the pod joined the pool.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets when health was last checked.
    /// </summary>
    public DateTime? LastHealthCheckAt { get; set; }

    /// <summary>
    /// Gets or sets when draining started.
    /// </summary>
    public DateTime? DrainingStartedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of active streams.
    /// </summary>
    public int ActiveStreams { get; set; }

    /// <summary>
    /// Gets or sets an optional affinity tag for sticky routing.
    /// </summary>
    public string? AffinityTag { get; set; }

    /// <summary>
    /// Gets the pod pool.
    /// </summary>
    public PodPool PodPool { get; set; } = null!;

    /// <summary>
    /// Gets the GPU pod.
    /// </summary>
    public GpuPod GpuPod { get; set; } = null!;
}
