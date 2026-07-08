namespace PodPilot.Application.Models.Lifecycle;

/// <summary>
/// Result of a pod shutdown operation.
/// </summary>
public sealed class PodShutdownResult
{
    /// <summary>
    /// Gets or sets whether the shutdown succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the resulting pod status.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets or sets an error message when the operation fails.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
