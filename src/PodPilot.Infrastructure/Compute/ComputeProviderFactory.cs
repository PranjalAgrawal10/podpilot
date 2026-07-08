using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Compute;

/// <summary>
/// Resolves compute provider implementations by type.
/// </summary>
public sealed class ComputeProviderFactory : IComputeProviderFactory
{
    private readonly IEnumerable<IComputeProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComputeProviderFactory"/> class.
    /// </summary>
    /// <param name="providers">Registered compute provider implementations.</param>
    public ComputeProviderFactory(IEnumerable<IComputeProvider> providers)
    {
        this.providers = providers;
    }

    /// <inheritdoc />
    public IComputeProvider GetProvider(ProviderType providerType)
    {
        var provider = providers.FirstOrDefault(p => p.ProviderType == providerType);
        if (provider is null)
        {
            throw new NotSupportedException($"Provider type '{providerType}' is not supported.");
        }

        return provider;
    }
}
