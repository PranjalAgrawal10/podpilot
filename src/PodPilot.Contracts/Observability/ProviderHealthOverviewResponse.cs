namespace PodPilot.Contracts.Observability;

/// <summary>
/// Provider health overview response.
/// </summary>
public sealed class ProviderHealthOverviewResponse
{
    /// <summary>Gets or sets when health was checked.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Gets or sets total provider count.</summary>
    public int TotalProviders { get; init; }

    /// <summary>Gets or sets healthy provider count.</summary>
    public int HealthyProviders { get; init; }

    /// <summary>Gets or sets unhealthy provider count.</summary>
    public int UnhealthyProviders { get; init; }

    /// <summary>Gets or sets per-provider health entries.</summary>
    public IReadOnlyList<ProviderHealthEntryResponse> Providers { get; init; } = [];
}

/// <summary>
/// Provider health entry response.
/// </summary>
public sealed class ProviderHealthEntryResponse
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider name.</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>Gets or sets the health status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets or sets response time in milliseconds.</summary>
    public int? ResponseTimeMs { get; init; }

    /// <summary>Gets or sets the optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets when health was last checked.</summary>
    public DateTime? LastCheckedAt { get; init; }
}
