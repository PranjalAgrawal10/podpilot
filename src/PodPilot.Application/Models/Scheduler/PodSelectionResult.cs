namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Result of pod selection for a scheduled request.
/// </summary>
public sealed class PodSelectionResult
{
    /// <summary>
    /// Gets or sets the selected pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

    /// <summary>
    /// Gets or sets the upstream base URL.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved model name.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid? ModelId { get; init; }

    /// <summary>
    /// Gets or sets the current load on the pod.
    /// </summary>
    public int CurrentLoad { get; init; }
}
