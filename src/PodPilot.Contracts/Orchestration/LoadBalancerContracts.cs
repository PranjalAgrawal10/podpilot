namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Load balancer configuration response.
/// </summary>
public sealed class LoadBalancerConfigResponse
{
    /// <summary>Gets or sets the strategy.</summary>
    public string Strategy { get; set; } = "LeastBusy";

    /// <summary>Gets or sets a value indicating whether sticky sessions are enabled.</summary>
    public bool StickySessionsEnabled { get; set; }

    /// <summary>Gets or sets sticky session TTL in minutes.</summary>
    public int StickySessionTtlMinutes { get; set; } = 30;
}

/// <summary>
/// Request to update load balancer configuration.
/// </summary>
public sealed class UpdateLoadBalancerConfigRequest
{
    /// <summary>Gets or sets the strategy.</summary>
    public string Strategy { get; set; } = "LeastBusy";

    /// <summary>Gets or sets a value indicating whether sticky sessions are enabled.</summary>
    public bool StickySessionsEnabled { get; set; }

    /// <summary>Gets or sets sticky session TTL in minutes.</summary>
    public int StickySessionTtlMinutes { get; set; } = 30;
}
