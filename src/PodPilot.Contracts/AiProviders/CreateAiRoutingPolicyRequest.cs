namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// Request to create an AI routing policy.
/// </summary>
public sealed class CreateAiRoutingPolicyRequest
{
    /// <summary>Gets or sets the policy name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the model name this policy applies to.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets the primary provider identifier.</summary>
    public Guid PrimaryProviderId { get; set; }

    /// <summary>Gets or sets ordered fallback provider identifiers.</summary>
    public IReadOnlyList<Guid> FallbackProviderIds { get; set; } = [];

    /// <summary>Gets or sets the failover strategy.</summary>
    public string FailoverStrategy { get; set; } = "RetryThenFailover";

    /// <summary>Gets or sets max retries on the primary provider.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Gets or sets a value indicating whether the policy is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether this is the default policy.</summary>
    public bool IsDefault { get; set; }
}
