namespace PodPilot.Contracts.Pods;

/// <summary>
/// Request to update a GPU pod.
/// </summary>
public sealed class UpdatePodRequest
{
    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }
}
