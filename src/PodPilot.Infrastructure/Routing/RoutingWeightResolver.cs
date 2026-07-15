using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Routing.Strategies;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Composes weight strategies to resolve scoring weights without modifying callers.
/// </summary>
public sealed class RoutingWeightResolver : IRoutingWeightResolver
{
    private readonly IReadOnlyList<IRoutingWeightStrategy> strategies;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingWeightResolver"/> class.
    /// </summary>
    public RoutingWeightResolver(IEnumerable<IRoutingWeightStrategy> strategies)
    {
        this.strategies = strategies.ToList();
    }

    /// <inheritdoc />
    public RoutingScoreWeights Resolve(AiRoutingPolicy? policy, RoutingStrategy strategy)
    {
        var effective = strategy == RoutingStrategy.OrganizationRules && policy is not null
            ? policy.Strategy
            : strategy;

        var match = strategies.FirstOrDefault(s => s.CanHandle(effective));
        return match?.Resolve(policy) ?? RoutingScoreWeights.Balanced;
    }
}
