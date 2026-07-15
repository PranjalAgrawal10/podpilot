using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Routing and failover policy for AI inference requests.
/// </summary>
public class AiRoutingPolicy : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the policy name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the model name this policy applies to (null = default / auto).</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets the primary AI provider identifier (required for provider-priority strategies).</summary>
    public Guid? PrimaryProviderId { get; set; }

    /// <summary>Gets or sets ordered fallback provider IDs as JSON array of GUIDs.</summary>
    public string FallbackProviderIdsJson { get; set; } = "[]";

    /// <summary>Gets or sets the failover strategy.</summary>
    public AiFailoverStrategy FailoverStrategy { get; set; } = AiFailoverStrategy.RetryThenFailover;

    /// <summary>Gets or sets the maximum retry attempts on the primary provider.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Gets or sets the intelligent routing strategy.</summary>
    public RoutingStrategy Strategy { get; set; } = RoutingStrategy.Balanced;

    /// <summary>Gets or sets cost scoring weight (0–1).</summary>
    public double CostWeight { get; set; } = 0.25;

    /// <summary>Gets or sets latency scoring weight (0–1).</summary>
    public double LatencyWeight { get; set; } = 0.25;

    /// <summary>Gets or sets reliability scoring weight (0–1).</summary>
    public double ReliabilityWeight { get; set; } = 0.20;

    /// <summary>Gets or sets context-window scoring weight (0–1).</summary>
    public double ContextWeight { get; set; } = 0.10;

    /// <summary>Gets or sets feature scoring weight (0–1).</summary>
    public double FeaturesWeight { get; set; } = 0.10;

    /// <summary>Gets or sets availability scoring weight (0–1).</summary>
    public double AvailabilityWeight { get; set; } = 0.10;

    /// <summary>Gets or sets preferred task types as JSON array of AiTaskType names.</summary>
    public string PreferredTaskTypesJson { get; set; } = "[]";

    /// <summary>Gets or sets custom rule expressions as JSON (optional filters).</summary>
    public string? CustomRulesJson { get; set; }

    /// <summary>Gets or sets a value indicating whether the policy is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether this is the default policy.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets the primary provider when set.</summary>
    public AiInferenceProvider? PrimaryProvider { get; set; }
}
