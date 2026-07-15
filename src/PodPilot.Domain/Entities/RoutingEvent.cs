using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit record for an intelligent routing decision.
/// </summary>
public class RoutingEvent : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the routing policy identifier when applicable.</summary>
    public Guid? RoutingPolicyId { get; set; }

    /// <summary>Gets or sets the classified task type.</summary>
    public AiTaskType TaskType { get; set; }

    /// <summary>Gets or sets the estimated task complexity.</summary>
    public TaskComplexity Complexity { get; set; }

    /// <summary>Gets or sets the strategy used for selection.</summary>
    public RoutingStrategy Strategy { get; set; }

    /// <summary>Gets or sets the selected provider identifier.</summary>
    public Guid? SelectedProviderId { get; set; }

    /// <summary>Gets or sets the selected model name.</summary>
    public string? SelectedModelName { get; set; }

    /// <summary>Gets or sets the overall score of the selected candidate.</summary>
    public double? OverallScore { get; set; }

    /// <summary>Gets or sets estimated input tokens.</summary>
    public int EstimatedInputTokens { get; set; }

    /// <summary>Gets or sets estimated output tokens.</summary>
    public int EstimatedOutputTokens { get; set; }

    /// <summary>Gets or sets estimated cost in USD.</summary>
    public decimal EstimatedCostUsd { get; set; }

    /// <summary>Gets or sets estimated latency in milliseconds.</summary>
    public int EstimatedLatencyMs { get; set; }

    /// <summary>Gets or sets the fallback attempt count for this decision.</summary>
    public int FallbackCount { get; set; }

    /// <summary>Gets or sets a value indicating whether this was a simulation.</summary>
    public bool IsSimulation { get; set; }

    /// <summary>Gets or sets an optional gateway request identifier.</summary>
    public Guid? GatewayRequestId { get; set; }

    /// <summary>Gets or sets a short decision reason.</summary>
    public string? DecisionReason { get; set; }

    /// <summary>Gets or sets when the decision was made.</summary>
    public DateTime DecidedAt { get; set; }

    /// <summary>Gets the routing policy.</summary>
    public AiRoutingPolicy? RoutingPolicy { get; set; }

    /// <summary>Gets the selected provider.</summary>
    public AiInferenceProvider? SelectedProvider { get; set; }
}
