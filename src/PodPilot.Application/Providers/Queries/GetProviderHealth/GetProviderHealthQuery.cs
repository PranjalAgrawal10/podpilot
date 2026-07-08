using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Queries.GetProviderHealth;

/// <summary>
/// Gets health information for a compute provider.
/// </summary>
public sealed class GetProviderHealthQuery : IRequest<ProviderHealthResponse>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }
}
