using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Tracks a model pull/download operation.
/// </summary>
public class ModelDownload : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Gets or sets the download progress percentage (0-100).
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the download status.
    /// </summary>
    public ModelDownloadStatus Status { get; set; } = ModelDownloadStatus.Queued;

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
    /// Gets or sets an optional error message when the download failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the associated model.
    /// </summary>
    public AiModel Model { get; set; } = null!;
}
