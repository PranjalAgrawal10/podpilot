namespace PodPilot.Contracts.Orchestration;

/// <summary>
/// Request to trigger manual scaling.
/// </summary>
public sealed class ManualScaleRequest
{
    /// <summary>Gets or sets the pod pool identifier.</summary>
    public Guid PoolId { get; set; }

    /// <summary>Gets or sets an optional reason.</summary>
    public string? Reason { get; set; }
}
