using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Per-organization load balancer configuration.
/// </summary>
public class LoadBalancerConfig : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the load balancing strategy.
    /// </summary>
    public LoadBalancingStrategy Strategy { get; set; } = LoadBalancingStrategy.LeastBusy;

    /// <summary>
    /// Gets or sets a value indicating whether sticky sessions are enabled.
    /// </summary>
    public bool StickySessionsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the sticky session TTL in minutes.
    /// </summary>
    public int StickySessionTtlMinutes { get; set; } = 30;

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;
}
