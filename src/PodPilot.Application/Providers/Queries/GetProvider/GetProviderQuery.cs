using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Queries.GetProvider;

/// <summary>
/// Gets a compute provider by identifier.
/// </summary>
public sealed class GetProviderQuery : IRequest<ProviderResponse>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }
}
