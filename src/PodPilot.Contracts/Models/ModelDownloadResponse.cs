namespace PodPilot.Contracts.Models;

/// <summary>
/// Model download progress response.
/// </summary>
public sealed class ModelDownloadResponse
{
    /// <summary>
    /// Gets or sets the download identifier.
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
    /// Gets or sets the progress percentage.
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the download status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the download speed in bytes per second.
    /// </summary>
    public long? DownloadSpeed { get; set; }

    /// <summary>
    /// Gets or sets when the download started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the download completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
