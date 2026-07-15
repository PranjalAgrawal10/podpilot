using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing.Strategies;

/// <summary>
/// Resolves scoring weights for one or more routing strategies (OCP).
/// </summary>
public interface IRoutingWeightStrategy
{
    /// <summary>Gets a value indicating whether this strategy handles the given routing strategy.</summary>
    bool CanHandle(RoutingStrategy strategy);

    /// <summary>Resolves weights, optionally using policy custom weights.</summary>
    RoutingScoreWeights Resolve(AiRoutingPolicy? policy);
}
