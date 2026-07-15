using MediatR;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Routing.Commands.UpdateRoutingPolicySettings;

/// <summary>Updates organization intelligent routing policy settings.</summary>
public sealed class UpdateRoutingPolicySettingsCommand : IRequest<RoutingPolicySettingsResponse>
{
    /// <summary>Gets or sets the routing strategy.</summary>
    public RoutingStrategy Strategy { get; init; } = RoutingStrategy.Balanced;

    /// <summary>Gets or sets cost weight.</summary>
    public double CostWeight { get; init; } = 0.25;

    /// <summary>Gets or sets latency weight.</summary>
    public double LatencyWeight { get; init; } = 0.25;

    /// <summary>Gets or sets reliability weight.</summary>
    public double ReliabilityWeight { get; init; } = 0.20;

    /// <summary>Gets or sets context weight.</summary>
    public double ContextWeight { get; init; } = 0.10;

    /// <summary>Gets or sets features weight.</summary>
    public double FeaturesWeight { get; init; } = 0.10;

    /// <summary>Gets or sets availability weight.</summary>
    public double AvailabilityWeight { get; init; } = 0.10;

    /// <summary>Gets or sets max retries.</summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>Gets or sets the failover strategy.</summary>
    public AiFailoverStrategy FailoverStrategy { get; init; } = AiFailoverStrategy.RetryThenFailover;

    /// <summary>Gets or sets the optional primary provider identifier.</summary>
    public Guid? PrimaryProviderId { get; init; }

    /// <summary>Gets or sets fallback provider identifiers.</summary>
    public IReadOnlyList<Guid> FallbackProviderIds { get; init; } = [];

    /// <summary>Gets or sets preferred task types.</summary>
    public IReadOnlyList<string> PreferredTaskTypes { get; init; } = [];

    /// <summary>Gets or sets optional custom rules JSON.</summary>
    public string? CustomRulesJson { get; init; }
}
