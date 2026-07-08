using PodPilot.Application.Models.Compute;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates compute provider operations with persistence.
/// </summary>
public interface IProviderService
{
    /// <summary>
    /// Validates credentials for a stored provider.
    /// </summary>
    Task<ProviderValidationResult> ValidateProviderAsync(ComputeProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates credentials using a provider type and raw API key.
    /// </summary>
    Task<ProviderValidationResult> ValidateCredentialsAsync(
        Domain.Enums.ProviderType providerType,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks health for a stored provider and persists the result.
    /// </summary>
    Task<ProviderHealthResult> CheckAndPersistHealthAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs regions and GPUs from the provider API into the database.
    /// </summary>
    Task SyncCatalogAsync(ComputeProvider provider, string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the decrypted API key for a provider.
    /// </summary>
    Task<string> GetDecryptedApiKeyAsync(ComputeProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists regions from the provider API.
    /// </summary>
    Task<IReadOnlyList<ProviderRegionInfo>> ListRegionsAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists GPUs from the provider API.
    /// </summary>
    Task<IReadOnlyList<ProviderGpuInfo>> ListGpusAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists templates from the provider API.
    /// </summary>
    Task<IReadOnlyList<ProviderTemplateInfo>> ListTemplatesAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets account information from the provider API.
    /// </summary>
    Task<ProviderAccountInfo> GetAccountInfoAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default);
}
