namespace PodPilot.Contracts.Models;

/// <summary>
/// Model management dashboard summary.
/// </summary>
public sealed class ModelDashboardResponse
{
    /// <summary>
    /// Gets or sets the installed model count.
    /// </summary>
    public int InstalledModels { get; set; }

    /// <summary>
    /// Gets or sets the number of active downloads.
    /// </summary>
    public int DownloadingModels { get; set; }

    /// <summary>
    /// Gets or sets the default model name if configured.
    /// </summary>
    public string? DefaultModel { get; set; }

    /// <summary>
    /// Gets or sets total storage used in bytes.
    /// </summary>
    public long StorageUsedBytes { get; set; }

    /// <summary>
    /// Gets or sets the detected Ollama version.
    /// </summary>
    public string? OllamaVersion { get; set; }

    /// <summary>
    /// Gets or sets whether Ollama was detected on the selected pod.
    /// </summary>
    public bool OllamaDetected { get; set; }

    /// <summary>
    /// Gets or sets the number of healthy models.
    /// </summary>
    public int HealthyModels { get; set; }

    /// <summary>
    /// Gets or sets the number of unhealthy models.
    /// </summary>
    public int UnhealthyModels { get; set; }
}
