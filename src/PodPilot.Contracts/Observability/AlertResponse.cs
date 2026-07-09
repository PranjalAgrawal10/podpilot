namespace PodPilot.Contracts.Observability;

/// <summary>
/// Alert history response.
/// </summary>
public sealed class AlertResponse
{
    /// <summary>Gets or sets the alert identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets when the alert was raised.</summary>
    public DateTime RaisedAt { get; init; }

    /// <summary>Gets or sets when the alert was resolved.</summary>
    public DateTime? ResolvedAt { get; init; }

    /// <summary>Gets or sets the alert type.</summary>
    public string AlertType { get; init; } = string.Empty;

    /// <summary>Gets or sets the alert severity.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets or sets the alert title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets or sets the alert message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? GpuPodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets a value indicating whether the alert is active.</summary>
    public bool IsActive { get; init; }
}
