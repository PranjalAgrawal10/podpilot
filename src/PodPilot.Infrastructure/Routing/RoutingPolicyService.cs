using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Resolves active routing policies and scoring weights.
/// </summary>
public sealed class RoutingPolicyService : IRoutingPolicy
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingPolicyService"/> class.
    /// </summary>
    public RoutingPolicyService(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AiRoutingPolicy?> GetActivePolicyAsync(
        Guid organizationId,
        string? modelHint,
        CancellationToken cancellationToken = default)
    {
        var policies = await dbContext.AiRoutingPolicies
            .AsNoTracking()
            .Include(p => p.PrimaryProvider)
            .Where(p => p.OrganizationId == organizationId && p.IsEnabled)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(modelHint) &&
            !string.Equals(modelHint, "auto", StringComparison.OrdinalIgnoreCase))
        {
            var modelMatch = policies.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.ModelName) &&
                string.Equals(p.ModelName, modelHint, StringComparison.OrdinalIgnoreCase));
            if (modelMatch is not null)
            {
                return modelMatch;
            }
        }

        return policies.FirstOrDefault(p => p.IsDefault)
               ?? policies.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.ModelName))
               ?? policies.FirstOrDefault();
    }

    /// <inheritdoc />
    public RoutingScoreWeights GetWeights(AiRoutingPolicy? policy, RoutingStrategy strategy)
    {
        var effective = strategy == RoutingStrategy.OrganizationRules && policy is not null
            ? policy.Strategy
            : strategy;

        if (effective is RoutingStrategy.CustomRules or RoutingStrategy.OrganizationRules ||
            (policy is not null && effective == policy.Strategy &&
             policy.Strategy is RoutingStrategy.CustomRules or RoutingStrategy.Balanced &&
             HasCustomWeights(policy)))
        {
            if (policy is not null && HasCustomWeights(policy))
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
        }

        return effective switch
        {
            RoutingStrategy.LowestCost => RoutingScoreWeights.LowestCost,
            RoutingStrategy.LowestLatency => RoutingScoreWeights.LowestLatency,
            RoutingStrategy.HighestAccuracy => RoutingScoreWeights.HighestAccuracy,
            RoutingStrategy.CustomRules when policy is not null => new RoutingScoreWeights
            {
                Cost = policy.CostWeight,
                Latency = policy.LatencyWeight,
                Reliability = policy.ReliabilityWeight,
                Context = policy.ContextWeight,
                Features = policy.FeaturesWeight,
                Availability = policy.AvailabilityWeight,
            },
            _ => policy is not null
                ? new RoutingScoreWeights
                {
                    Cost = policy.CostWeight,
                    Latency = policy.LatencyWeight,
                    Reliability = policy.ReliabilityWeight,
                    Context = policy.ContextWeight,
                    Features = policy.FeaturesWeight,
                    Availability = policy.AvailabilityWeight,
                }
                : RoutingScoreWeights.Balanced,
        };
    }

    private static bool HasCustomWeights(AiRoutingPolicy policy) =>
        Math.Abs(policy.CostWeight + policy.LatencyWeight + policy.ReliabilityWeight +
                 policy.ContextWeight + policy.FeaturesWeight + policy.AvailabilityWeight - 1.0) < 0.02;
}
