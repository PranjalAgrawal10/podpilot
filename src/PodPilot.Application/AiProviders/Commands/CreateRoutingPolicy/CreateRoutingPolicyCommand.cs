using MediatR;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.CreateRoutingPolicy;

/// <summary>
/// Creates an AI routing policy.
/// </summary>
public sealed class CreateRoutingPolicyCommand : IRequest<AiRoutingPolicyResponse>
{
    /// <summary>Gets or sets the policy name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the model name.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets or sets the primary provider identifier.</summary>
    public Guid PrimaryProviderId { get; init; }

    /// <summary>Gets or sets fallback provider identifiers.</summary>
    public IReadOnlyList<Guid> FallbackProviderIds { get; init; } = [];

    /// <summary>Gets or sets the failover strategy.</summary>
    public AiFailoverStrategy FailoverStrategy { get; init; } = AiFailoverStrategy.RetryThenFailover;

    /// <summary>Gets or sets max retries.</summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>Gets or sets whether the policy is enabled.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Gets or sets whether this is the default policy.</summary>
    public bool IsDefault { get; init; }
}
