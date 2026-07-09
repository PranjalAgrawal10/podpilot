namespace PodPilot.Contracts.Observability;

/// <summary>
/// Pod health overview response.
/// </summary>
public sealed class PodHealthOverviewResponse
{
    /// <summary>Gets or sets when health was checked.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Gets or sets total pod count.</summary>
    public int TotalPods { get; init; }

    /// <summary>Gets or sets healthy pod count.</summary>
    public int HealthyPods { get; init; }

    /// <summary>Gets or sets degraded pod count.</summary>
    public int DegradedPods { get; init; }

    /// <summary>Gets or sets unhealthy pod count.</summary>
    public int UnhealthyPods { get; init; }

    /// <summary>Gets or sets per-pod health entries.</summary>
    public IReadOnlyList<PodHealthEntryResponse> Pods { get; init; } = [];
}

/// <summary>
/// Pod health entry response.
/// </summary>
public sealed class PodHealthEntryResponse
{
    /// <summary>Gets or sets the pod identifier.</summary>
    public Guid PodId { get; init; }

    /// <summary>Gets or sets the pod name.</summary>
    public string PodName { get; init; } = string.Empty;

    /// <summary>Gets or sets the health status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets or sets GPU health.</summary>
    public bool GpuHealthy { get; init; }

    /// <summary>Gets or sets Ollama health.</summary>
    public bool OllamaHealthy { get; init; }

    /// <summary>Gets or sets models health.</summary>
    public bool ModelsHealthy { get; init; }

    /// <summary>Gets or sets latency in milliseconds.</summary>
    public int LatencyMs { get; init; }

    /// <summary>Gets or sets GPU utilization percentage.</summary>
    public double? GpuUtilizationPercent { get; init; }

    /// <summary>Gets or sets the optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime? LastCheckedAt { get; init; }
}
