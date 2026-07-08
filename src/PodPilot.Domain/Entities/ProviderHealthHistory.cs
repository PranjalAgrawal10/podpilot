using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Historical health check record for a compute provider.
/// </summary>
public class ProviderHealthHistory : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the compute provider identifier.
    /// </summary>
    public Guid ComputeProviderId { get; set; }

    /// <summary>
    /// Gets or sets the connection status at check time.
    /// </summary>
    public ProviderConnectionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the health check occurred.
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds.
    /// </summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the compute provider.
    /// </summary>
    public ComputeProvider ComputeProvider { get; set; } = null!;
}
