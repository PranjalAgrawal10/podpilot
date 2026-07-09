using MediatR;

namespace PodPilot.Application.Orchestration.Commands.DeletePodPool;

/// <summary>
/// Deletes a pod pool.
/// </summary>
public sealed class DeletePodPoolCommand : IRequest
{
    /// <summary>Gets or sets the pool identifier.</summary>
    public Guid PoolId { get; init; }
}
