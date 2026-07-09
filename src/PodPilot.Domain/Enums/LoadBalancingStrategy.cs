namespace PodPilot.Domain.Enums;

/// <summary>
/// Load balancing strategy for pod selection.
/// </summary>
public enum LoadBalancingStrategy
{
    /// <summary>Round-robin across healthy pods.</summary>
    RoundRobin = 0,

    /// <summary>Select pod with lowest active request count.</summary>
    LeastBusy = 1,

    /// <summary>Select pod with shortest queue depth.</summary>
    LeastQueue = 2,

    /// <summary>Select pod with lowest measured latency.</summary>
    LowestLatency = 3,

    /// <summary>Weighted distribution based on pod weights.</summary>
    Weighted = 4,

    /// <summary>Sticky sessions route to the same pod.</summary>
    StickySession = 5,
}
