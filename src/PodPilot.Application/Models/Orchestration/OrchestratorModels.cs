using PodPilot.Domain.Entities;

namespace PodPilot.Application.Models.Orchestration;

/// <summary>
/// Request to resolve a pod via orchestration.
/// </summary>
public sealed class OrchestratorRouteRequest
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets an optional preferred pod identifier.</summary>
    public Guid? PreferredPodId { get; init; }

    /// <summary>Gets or sets an optional sticky session key.</summary>
    public string? SessionKey { get; init; }
}

/// <summary>
/// Result of orchestrator pod resolution.
/// </summary>
public sealed class OrchestratorRouteResult
{
    /// <summary>Gets or sets the selected pod.</summary>
    public GpuPod Pod { get; init; } = null!;

    /// <summary>Gets or sets the base URL.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets the resolved model.</summary>
    public string? Model { get; init; }

    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid? PoolId { get; init; }

    /// <summary>Gets or sets the current load on the pod.</summary>
    public int CurrentLoad { get; init; }
}

/// <summary>
/// Orchestrator status summary.
/// </summary>
public sealed class OrchestratorStatus
{
    /// <summary>Gets or sets the total pool count.</summary>
    public int PoolCount { get; init; }

    /// <summary>Gets or sets the running pod count.</summary>
    public int RunningPods { get; init; }

    /// <summary>Gets or sets the healthy pod count.</summary>
    public int HealthyPods { get; init; }

    /// <summary>Gets or sets the draining pod count.</summary>
    public int DrainingPods { get; init; }

    /// <summary>Gets or sets the failed pod count.</summary>
    public int FailedPods { get; init; }

    /// <summary>Gets or sets the current queue length.</summary>
    public int QueueLength { get; init; }

    /// <summary>Gets or sets the average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets requests per second.</summary>
    public double RequestsPerSecond { get; init; }
}

/// <summary>
/// Result of a failover operation.
/// </summary>
public sealed class FailoverResult
{
    /// <summary>Gets or sets a value indicating whether failover succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Gets or sets the replacement pod identifier.</summary>
    public Guid? ReplacementPodId { get; init; }

    /// <summary>Gets or sets the number of reassigned requests.</summary>
    public int ReassignedRequestCount { get; init; }

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; init; }
}
