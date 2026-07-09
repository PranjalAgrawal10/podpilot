using MediatR;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Commands.ScaleUp;

/// <summary>
/// Manually scales up a pod pool.
/// </summary>
public sealed class ScaleUpCommand : IRequest<ScalingActionResult>
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }

    /// <summary>Gets or sets the reason.</summary>
    public string? Reason { get; init; }
}
