using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Queries.ListProviderTemplates;

/// <summary>
/// Lists templates for a compute provider.
/// </summary>
public sealed class ListProviderTemplatesQuery : IRequest<IReadOnlyList<ProviderTemplateResponse>>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }
}
