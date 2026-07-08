using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Compute;

/// <summary>
/// Health check result from a compute provider.
/// </summary>
public sealed class ProviderHealthResult
{
    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public ProviderConnectionStatus Status { get; init; }

    /// <summary>
    /// Gets or sets the response time in milliseconds.
    /// </summary>
    public int? ResponseTimeMs { get; init; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets when the check was performed.
    /// </summary>
    public DateTime CheckedAt { get; init; }
}
