using MediatR;

namespace PodPilot.Application.Models.Commands.DeleteModel;

/// <summary>
/// Deletes a model from a GPU pod.
/// </summary>
public sealed class DeleteModelCommand : IRequest
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to delete the default model.
    /// </summary>
    public bool ForceDefault { get; set; }
}
