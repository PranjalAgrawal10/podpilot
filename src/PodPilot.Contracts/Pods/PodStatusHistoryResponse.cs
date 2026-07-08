namespace PodPilot.Contracts.Pods;

/// <summary>
/// Pod status history response.
/// </summary>
public sealed class PodStatusHistoryResponse
{
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets when the status was recorded.
    /// </summary>
    public DateTime RecordedAt { get; init; }

    /// <summary>
    /// Gets or sets an optional message.
    /// </summary>
    public string? Message { get; init; }
}
