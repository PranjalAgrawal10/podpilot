using MediatR;

namespace PodPilot.Application.Scheduler.Commands.CancelSchedulerRequest;

/// <summary>
/// Cancels a scheduler request.
/// </summary>
public sealed class CancelSchedulerRequestCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public Guid RequestId { get; init; }
}
