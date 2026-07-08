using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Commands.PullModel;

/// <summary>
/// Starts pulling a model to a GPU pod.
/// </summary>
public sealed class PullModelCommand : IRequest<ModelDownloadResponse>
{
    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the model reference.
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
