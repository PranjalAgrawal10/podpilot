using MediatR;

namespace PodPilot.Application.AiProviders.Commands.DeleteAiProvider;

/// <summary>
/// Deletes an AI inference provider.
/// </summary>
public sealed class DeleteAiProviderCommand : IRequest<Unit>
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }
}
