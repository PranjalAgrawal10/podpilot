namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Orchestrator status response.
/// </summary>
public sealed class OrchestratorStatusResponse
{
    /// <summary>Gets or sets the pool count.</summary>
    public int PoolCount { get; set; }

    /// <summary>Gets or sets running pods.</summary>
    public int RunningPods { get; set; }

    /// <summary>Gets or sets healthy pods.</summary>
    public int HealthyPods { get; set; }

    /// <summary>Gets or sets draining pods.</summary>
    public int DrainingPods { get; set; }

    /// <summary>Gets or sets failed pods.</summary>
    public int FailedPods { get; set; }

    /// <summary>Gets or sets queue length.</summary>
    public int QueueLength { get; set; }

    /// <summary>Gets or sets average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>Gets or sets requests per second.</summary>
    public double RequestsPerSecond { get; set; }
}
