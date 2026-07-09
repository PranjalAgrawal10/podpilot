namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Scaling event response.
/// </summary>
public sealed class ScalingEventResponse
{
    /// <summary>Gets or sets the event identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the pod pool identifier.</summary>
    public Guid? PodPoolId { get; set; }

    /// <summary>Gets or sets the GPU pod identifier.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets the direction.</summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>Gets or sets the trigger type.</summary>
    public string TriggerType { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether scaling succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets when the event occurred.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Gets or sets pod count before scaling.</summary>
    public int PodCountBefore { get; set; }

    /// <summary>Gets or sets pod count after scaling.</summary>
    public int PodCountAfter { get; set; }
}
