using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Commands.SetDefaultModel;

/// <summary>
/// Sets the default model for a pod.
/// </summary>
public sealed class SetDefaultModelCommand : IRequest<ModelResponse>
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid ModelId { get; set; }
}
