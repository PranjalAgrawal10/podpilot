using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Current health snapshot for a compute provider.
/// </summary>
public class ProviderHealth : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the compute provider identifier.
    /// </summary>
    public Guid ComputeProviderId { get; set; }

    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public ProviderConnectionStatus Status { get; set; } = ProviderConnectionStatus.Unknown;

    /// <summary>
    /// Gets or sets when health was last checked.
    /// </summary>
    public DateTime? LastCheckedAt { get; set; }

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
