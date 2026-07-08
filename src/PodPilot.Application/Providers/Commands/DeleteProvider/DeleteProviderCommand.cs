using MediatR;

namespace PodPilot.Application.Providers.Commands.DeleteProvider;

/// <summary>
/// Deletes a compute provider.
/// </summary>
public sealed class DeleteProviderCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }
}
