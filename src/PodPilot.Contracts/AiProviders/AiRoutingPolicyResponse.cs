namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// AI routing policy response.
/// </summary>
public sealed class AiRoutingPolicyResponse
{
    /// <summary>Gets or sets the policy identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the policy name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the model name this policy applies to.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets the primary provider identifier.</summary>
    public Guid? PrimaryProviderId { get; init; }

    /// <summary>Gets or sets the intelligent routing strategy.</summary>
    public string Strategy { get; init; } = "ProviderPriority";

    /// <summary>Gets or sets the primary provider display name.</summary>
    public string? PrimaryProviderDisplayName { get; init; }

    /// <summary>Gets or sets ordered fallback provider identifiers.</summary>
    public IReadOnlyList<Guid> FallbackProviderIds { get; init; } = [];

    /// <summary>Gets or sets the failover strategy.</summary>
    public string FailoverStrategy { get; init; } = string.Empty;

    /// <summary>Gets or sets max retries on the primary provider.</summary>
    public int MaxRetries { get; init; }

    /// <summary>Gets or sets a value indicating whether the policy is enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets or sets a value indicating whether this is the default policy.</summary>
    public bool IsDefault { get; init; }

    /// <summary>Gets or sets when the policy was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets when the policy was last updated.</summary>
    public DateTime? UpdatedAt { get; init; }
}
