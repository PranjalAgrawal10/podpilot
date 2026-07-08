namespace PodPilot.Contracts.Pods;

/// <summary>
/// Request to delete a GPU pod.
/// </summary>
public sealed class DeletePodRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether to force delete a running pod.
    /// </summary>
    public bool Force { get; set; }
}
