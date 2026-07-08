namespace PodPilot.Contracts.Models;

/// <summary>
/// Detailed AI model response.
/// </summary>
public sealed class ModelDetailResponse : ModelResponse
{
    /// <summary>
    /// Gets or sets recent health checks.
    /// </summary>
    public IReadOnlyList<ModelHealthResponse> HealthHistory { get; set; } = [];

    /// <summary>
    /// Gets or sets recent downloads.
    /// </summary>
    public IReadOnlyList<ModelDownloadResponse> Downloads { get; set; } = [];
}
