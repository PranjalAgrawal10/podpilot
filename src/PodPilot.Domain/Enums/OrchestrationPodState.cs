namespace PodPilot.Domain.Enums;

/// <summary>
/// Orchestration lifecycle state for a pod within a pool.
/// </summary>
public enum OrchestrationPodState
{
    /// <summary>Pod is being provisioned on the provider.</summary>
    Provisioning = 0,

    /// <summary>Pod is starting.</summary>
    Starting = 1,

    /// <summary>Pod is warming up models and services.</summary>
    Warming = 2,

    /// <summary>Pod is healthy and ready to accept requests.</summary>
    Healthy = 3,

    /// <summary>Pod is actively processing requests.</summary>
    Busy = 4,

    /// <summary>Pod is draining active requests before shutdown.</summary>
    Draining = 5,

    /// <summary>Pod is stopping.</summary>
    Stopping = 6,

    /// <summary>Pod is stopped.</summary>
    Stopped = 7,

    /// <summary>Pod has failed health or lifecycle checks.</summary>
    Failed = 8,

    /// <summary>Pod is being deleted.</summary>
    Deleting = 9,
}
