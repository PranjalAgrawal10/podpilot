using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Orchestration;

/// <summary>
/// Request context for load balancer pod selection.
/// </summary>
public sealed class LoadBalancerRequest
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets an optional sticky session key.</summary>
    public string? SessionKey { get; init; }

    /// <summary>Gets or sets eligible pool members.</summary>
    public IReadOnlyList<PoolMemberContext> Members { get; init; } = [];
}

/// <summary>
/// Context for a pool member during load balancing.
/// </summary>
public sealed class PoolMemberContext
{
    /// <summary>Gets or sets the member identifier.</summary>
    public Guid MemberId { get; init; }

    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod entity.</summary>
    public GpuPod Pod { get; init; } = null!;

    /// <summary>Gets or sets the base URL.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets the orchestration state.</summary>
    public OrchestrationPodState State { get; init; }

    /// <summary>Gets or sets the load balancing weight.</summary>
    public int Weight { get; init; } = 1;

    /// <summary>Gets or sets the current request load.</summary>
    public int CurrentLoad { get; init; }

    /// <summary>Gets or sets the queue depth.</summary>
    public int QueueDepth { get; init; }

    /// <summary>Gets or sets the average latency in milliseconds.</summary>
    public int AverageLatencyMs { get; init; }

    /// <summary>Gets or sets the affinity tag.</summary>
    public string? AffinityTag { get; init; }

    /// <summary>Gets or sets a value indicating whether this is a warm standby.</summary>
    public bool IsWarmStandby { get; init; }
}

/// <summary>
/// Result of load balancer pod selection.
/// </summary>
public sealed class LoadBalancerSelection
{
    /// <summary>Gets or sets the selected pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the base URL.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets the current load.</summary>
    public int CurrentLoad { get; init; }

    /// <summary>Gets or sets the strategy used.</summary>
    public LoadBalancingStrategy Strategy { get; init; }
}

/// <summary>
/// Load balancer configuration DTO.
/// </summary>
public sealed class LoadBalancerConfigDto
{
    /// <summary>Gets or sets the strategy.</summary>
    public LoadBalancingStrategy Strategy { get; init; } = LoadBalancingStrategy.LeastBusy;

    /// <summary>Gets or sets a value indicating whether sticky sessions are enabled.</summary>
    public bool StickySessionsEnabled { get; init; }

    /// <summary>Gets or sets the sticky session TTL in minutes.</summary>
    public int StickySessionTtlMinutes { get; init; } = 30;
}
