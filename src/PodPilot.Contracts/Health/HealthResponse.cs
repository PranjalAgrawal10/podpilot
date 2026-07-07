namespace PodPilot.Contracts.Health;

/// <summary>
/// Health check response model.
/// </summary>
public sealed class HealthResponse
{
    /// <summary>
    /// Gets or sets the overall health status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets individual health check entries.
    /// </summary>
    public Dictionary<string, HealthCheckEntry> Checks { get; set; } = [];

    /// <summary>
    /// Gets or sets the total duration of all checks.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }
}

/// <summary>
/// Individual health check entry.
/// </summary>
public sealed class HealthCheckEntry
{
    /// <summary>
    /// Gets or sets the check status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the check duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
