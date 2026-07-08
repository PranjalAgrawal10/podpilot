using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Commands.RefreshModels;

/// <summary>
/// Refreshes models from Ollama for a pod.
/// </summary>
public sealed class RefreshModelsCommand : IRequest<IReadOnlyList<ModelResponse>>
{
    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid PodId { get; set; }
}
