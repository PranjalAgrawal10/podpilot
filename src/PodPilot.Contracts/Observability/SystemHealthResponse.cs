namespace PodPilot.Contracts.Observability;

/// <summary>
/// System health response.
/// </summary>
public sealed class SystemHealthResponse
{
    /// <summary>Gets or sets when health was checked.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Gets or sets overall health status.</summary>
    public string OverallStatus { get; init; } = string.Empty;

    /// <summary>Gets or sets component health entries.</summary>
    public IReadOnlyList<ComponentHealthResponse> Components { get; init; } = [];
}

/// <summary>
/// Component health response.
/// </summary>
public sealed class ComponentHealthResponse
{
    /// <summary>Gets or sets the component name.</summary>
    public string Component { get; init; } = string.Empty;

    /// <summary>Gets or sets the health status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets or sets the status message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional related entity identifier.</summary>
    public Guid? RelatedEntityId { get; init; }
}
