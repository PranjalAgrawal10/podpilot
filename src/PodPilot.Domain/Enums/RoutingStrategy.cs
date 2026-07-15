namespace PodPilot.Domain.Enums;

/// <summary>
/// Organization routing strategy for intelligent model selection.
/// </summary>
public enum RoutingStrategy
{
    /// <summary>Minimize predicted token and runtime cost.</summary>
    LowestCost = 0,

    /// <summary>Minimize predicted end-to-end latency.</summary>
    LowestLatency = 1,

    /// <summary>Prefer higher-capability / higher-accuracy models.</summary>
    HighestAccuracy = 2,

    /// <summary>Weighted balance of cost, latency, reliability, and features.</summary>
    Balanced = 3,

    /// <summary>Honor explicit primary provider and fallback list.</summary>
    ProviderPriority = 4,

    /// <summary>Use custom scoring weights and rules on the policy.</summary>
    CustomRules = 5,

    /// <summary>Use organization default scoring weights.</summary>
    OrganizationRules = 6,
}
