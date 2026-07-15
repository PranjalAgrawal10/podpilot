namespace PodPilot.Contracts.Routing;

/// <summary>
/// Organization intelligent routing policy settings.
/// </summary>
public sealed class RoutingPolicySettingsResponse
{
    /// <summary>Gets or sets the policy identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the policy name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the routing strategy.</summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>Gets or sets cost weight.</summary>
    public double CostWeight { get; init; }

    /// <summary>Gets or sets latency weight.</summary>
    public double LatencyWeight { get; init; }

    /// <summary>Gets or sets reliability weight.</summary>
    public double ReliabilityWeight { get; init; }

    /// <summary>Gets or sets context weight.</summary>
    public double ContextWeight { get; init; }

    /// <summary>Gets or sets features weight.</summary>
    public double FeaturesWeight { get; init; }

    /// <summary>Gets or sets availability weight.</summary>
    public double AvailabilityWeight { get; init; }

    /// <summary>Gets or sets max retries.</summary>
    public int MaxRetries { get; init; }

    /// <summary>Gets or sets the failover strategy.</summary>
    public string FailoverStrategy { get; init; } = string.Empty;

    /// <summary>Gets or sets whether this is the default policy.</summary>
    public bool IsDefault { get; init; }

    /// <summary>Gets or sets the optional primary provider identifier.</summary>
    public Guid? PrimaryProviderId { get; init; }

    /// <summary>Gets or sets fallback provider identifiers.</summary>
    public IReadOnlyList<Guid> FallbackProviderIds { get; init; } = [];

    /// <summary>Gets or sets preferred task types.</summary>
    public IReadOnlyList<string> PreferredTaskTypes { get; init; } = [];

    /// <summary>Gets or sets optional custom rules JSON.</summary>
    public string? CustomRulesJson { get; init; }
}
