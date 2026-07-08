using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Queries.ListProviderGpus;

/// <summary>
/// Lists GPUs for a compute provider.
/// </summary>
public sealed class ListProviderGpusQuery : IRequest<IReadOnlyList<ProviderGpuResponse>>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }
}
