using PodPilot.Application.Models.Compute;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Abstraction for a compute provider integration.
/// </summary>
public interface IComputeProvider
{
    /// <summary>
    /// Gets the provider type handled by this implementation.
    /// </summary>
    ProviderType ProviderType { get; }

    /// <summary>
    /// Validates the supplied API key.
    /// </summary>
    Task<ProviderValidationResult> ValidateCredentialsAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves account information.
    /// </summary>
    Task<ProviderAccountInfo> GetAccountInfoAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available regions.
    /// </summary>
    Task<IReadOnlyList<ProviderRegionInfo>> ListRegionsAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available GPU types.
    /// </summary>
    Task<IReadOnlyList<ProviderGpuInfo>> ListGpusAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available templates.
    /// </summary>
    Task<IReadOnlyList<ProviderTemplateInfo>> ListTemplatesAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a health check against the provider.
    /// </summary>
    Task<ProviderHealthResult> CheckHealthAsync(string apiKey, CancellationToken cancellationToken = default);
}
