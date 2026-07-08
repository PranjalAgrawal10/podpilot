using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Resolves compute provider implementations by type.
/// </summary>
public interface IComputeProviderFactory
{
    /// <summary>
    /// Gets the compute provider implementation for the specified type.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <returns>The provider implementation.</returns>
    IComputeProvider GetProvider(ProviderType providerType);
}
