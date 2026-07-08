namespace PodPilot.Contracts.Lifecycle;

/// <summary>
/// Shutdown operation response.
/// </summary>
public sealed class PodShutdownResponse
{
    /// <summary>
    /// Gets or sets whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the pod status.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets or sets an error message.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
