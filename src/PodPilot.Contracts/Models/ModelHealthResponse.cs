namespace PodPilot.Contracts.Models;

/// <summary>
/// Model health check response.
/// </summary>
public sealed class ModelHealthResponse
{
    /// <summary>
    /// Gets or sets the health record identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Gets or sets the model full name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the health status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response time in milliseconds.
    /// </summary>
    public int? ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets when the check occurred.
    /// </summary>
    public DateTime LastChecked { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
