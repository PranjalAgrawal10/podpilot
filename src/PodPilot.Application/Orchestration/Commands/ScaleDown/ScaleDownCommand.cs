using MediatR;
using PodPilot.Application.Models.Orchestration;

namespace PodPilot.Application.Orchestration.Commands.ScaleDown;

/// <summary>
/// Manually scales down a pod pool.
/// </summary>
public sealed class ScaleDownCommand : IRequest<ScalingActionResult>
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }

    /// <summary>Gets or sets the reason.</summary>
    public string? Reason { get; init; }
}
