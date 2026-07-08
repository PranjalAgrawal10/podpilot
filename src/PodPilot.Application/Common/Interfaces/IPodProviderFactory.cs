using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Resolves pod provider implementations by type.
/// </summary>
public interface IPodProviderFactory
{
    /// <summary>
    /// Gets the pod provider for the specified type.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <returns>The pod provider implementation.</returns>
    IPodProvider GetProvider(ProviderType providerType);
}
