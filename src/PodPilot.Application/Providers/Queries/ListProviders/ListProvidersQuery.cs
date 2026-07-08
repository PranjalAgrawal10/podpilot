using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Queries.ListProviders;

/// <summary>
/// Lists compute providers for the current organization.
/// </summary>
public sealed class ListProvidersQuery : IRequest<IReadOnlyList<ProviderResponse>>
{
}
