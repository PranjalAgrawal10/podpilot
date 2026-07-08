using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Historical health check record for an AI model.
/// </summary>
public class ModelHealthHistory : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Gets or sets the health status at check time.
    /// </summary>
    public ModelHealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds.
    /// </summary>
    public int? ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets when the health check occurred.
    /// </summary>
    public DateTime LastChecked { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the associated model.
    /// </summary>
    public AiModel Model { get; set; } = null!;
}
