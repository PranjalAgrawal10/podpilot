using MediatR;

namespace PodPilot.Application.AiProviders.Commands.DeleteRoutingPolicy;

/// <summary>
/// Deletes an AI routing policy.
/// </summary>
public sealed class DeleteRoutingPolicyCommand : IRequest<Unit>
{
    /// <summary>Gets or sets the policy identifier.</summary>
    public Guid PolicyId { get; init; }
}
