using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing.Strategies;

/// <summary>Lowest-cost weight strategy.</summary>
public sealed class LowestCostWeightStrategy : IRoutingWeightStrategy
{
    /// <inheritdoc />
    public bool CanHandle(RoutingStrategy strategy) => strategy == RoutingStrategy.LowestCost;

    /// <inheritdoc />
    public RoutingScoreWeights Resolve(AiRoutingPolicy? policy) => RoutingScoreWeights.LowestCost;
}

/// <summary>Lowest-latency weight strategy.</summary>
public sealed class LowestLatencyWeightStrategy : IRoutingWeightStrategy
{
    /// <inheritdoc />
    public bool CanHandle(RoutingStrategy strategy) => strategy == RoutingStrategy.LowestLatency;

    /// <inheritdoc />
    public RoutingScoreWeights Resolve(AiRoutingPolicy? policy) => RoutingScoreWeights.LowestLatency;
}

/// <summary>Highest-accuracy weight strategy.</summary>
public sealed class HighestAccuracyWeightStrategy : IRoutingWeightStrategy
{
    /// <inheritdoc />
    public bool CanHandle(RoutingStrategy strategy) => strategy == RoutingStrategy.HighestAccuracy;

    /// <inheritdoc />
    public RoutingScoreWeights Resolve(AiRoutingPolicy? policy) => RoutingScoreWeights.HighestAccuracy;
}

/// <summary>Uses organization policy custom weights when present.</summary>
public sealed class PolicyConfiguredWeightStrategy : IRoutingWeightStrategy
{
    /// <inheritdoc />
    public bool CanHandle(RoutingStrategy strategy) =>
        strategy is RoutingStrategy.Balanced
            or RoutingStrategy.CustomRules
            or RoutingStrategy.OrganizationRules
            or RoutingStrategy.ProviderPriority;

    /// <inheritdoc />
    public RoutingScoreWeights Resolve(AiRoutingPolicy? policy)
    {
        if (policy is not null && HasValidWeights(policy))
        {
            return new RoutingScoreWeights
            {
                Cost = policy.CostWeight,
                Latency = policy.LatencyWeight,
                Reliability = policy.ReliabilityWeight,
                Context = policy.ContextWeight,
                Features = policy.FeaturesWeight,
                Availability = policy.AvailabilityWeight,
            };
        }

        return RoutingScoreWeights.Balanced;
    }

    private static bool HasValidWeights(AiRoutingPolicy policy) =>
        Math.Abs(policy.CostWeight + policy.LatencyWeight + policy.ReliabilityWeight +
                 policy.ContextWeight + policy.FeaturesWeight + policy.AvailabilityWeight - 1.0) < 0.02;
}
