namespace PodPilot.Domain.Enums;

/// <summary>
/// Status of a model download operation.
/// </summary>
public enum ModelDownloadStatus
{
    /// <summary>Download is queued.</summary>
    Queued = 0,

    /// <summary>Download is in progress.</summary>
    Downloading = 1,

    /// <summary>Download completed successfully.</summary>
    Completed = 2,

    /// <summary>Download failed.</summary>
    Failed = 3,
}
