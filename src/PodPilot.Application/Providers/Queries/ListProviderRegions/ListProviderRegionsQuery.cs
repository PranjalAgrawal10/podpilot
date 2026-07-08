using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Queries.ListProviderRegions;

/// <summary>
/// Lists regions for a compute provider.
/// </summary>
public sealed class ListProviderRegionsQuery : IRequest<IReadOnlyList<ProviderRegionResponse>>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }
}
