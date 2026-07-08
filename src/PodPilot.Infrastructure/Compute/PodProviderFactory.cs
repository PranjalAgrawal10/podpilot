using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Compute;

/// <summary>
/// Resolves pod provider implementations by type.
/// </summary>
public sealed class PodProviderFactory : IPodProviderFactory
{
    private readonly IEnumerable<IPodProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodProviderFactory"/> class.
    /// </summary>
    /// <param name="providers">Registered pod providers.</param>
    public PodProviderFactory(IEnumerable<IPodProvider> providers)
    {
        this.providers = providers;
    }

    /// <inheritdoc />
    public IPodProvider GetProvider(ProviderType providerType) =>
        providers.FirstOrDefault(p => p.ProviderType == providerType)
        ?? throw new NotSupportedException($"Pod provider '{providerType}' is not supported.");
}
