using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Compute;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Orchestrates compute provider operations with persistence.
/// </summary>
public sealed class ProviderService : IProviderService
{
    private readonly IComputeProviderFactory computeProviderFactory;
    private readonly IEncryptionService encryptionService;
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderService"/> class.
    /// </summary>
    public ProviderService(
        IComputeProviderFactory computeProviderFactory,
        IEncryptionService encryptionService,
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService)
    {
        this.computeProviderFactory = computeProviderFactory;
        this.encryptionService = encryptionService;
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public Task<ProviderValidationResult> ValidateProviderAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetDecryptedApiKey(provider);
        return ValidateCredentialsAsync(provider.ProviderType, apiKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProviderValidationResult> ValidateCredentialsAsync(
        ProviderType providerType,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var computeProvider = computeProviderFactory.GetProvider(providerType);
        return await computeProvider.ValidateCredentialsAsync(apiKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResult> CheckAndPersistHealthAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync(provider, cancellationToken);
        var computeProvider = computeProviderFactory.GetProvider(provider.ProviderType);
        var result = await computeProvider.CheckHealthAsync(apiKey, cancellationToken);

        var health = await dbContext.ProviderHealthSnapshots
            .FirstOrDefaultAsync(h => h.ComputeProviderId == provider.Id, cancellationToken);

        if (health is null)
        {
            health = new ProviderHealth
            {
                ComputeProviderId = provider.Id,
            };

            await dbContext.AddProviderHealthAsync(health, cancellationToken);
        }

        health.Status = result.Status;
        health.LastCheckedAt = result.CheckedAt;
        health.ResponseTimeMs = result.ResponseTimeMs;
        health.ErrorMessage = result.ErrorMessage;

        await dbContext.AddProviderHealthHistoryAsync(
            new ProviderHealthHistory
            {
                ComputeProviderId = provider.Id,
                Status = result.Status,
                CheckedAt = result.CheckedAt,
                ResponseTimeMs = result.ResponseTimeMs,
                ErrorMessage = result.ErrorMessage,
            },
            cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task SyncCatalogAsync(
        ComputeProvider provider,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var computeProvider = computeProviderFactory.GetProvider(provider.ProviderType);
        var now = dateTimeService.UtcNow;

        var regions = await computeProvider.ListRegionsAsync(apiKey, cancellationToken);

        await dbContext.ClearProviderCatalogAsync(provider.Id, cancellationToken);

        foreach (var region in regions)
        {
            await dbContext.AddProviderRegionAsync(
                new ProviderRegion
                {
                    ComputeProviderId = provider.Id,
                    RegionId = region.RegionId,
                    Name = region.Name,
                    IsAvailable = region.IsAvailable,
                    SyncedAt = now,
                },
                cancellationToken);
        }

        var gpus = await computeProvider.ListGpusAsync(apiKey, cancellationToken);
        foreach (var gpu in gpus)
        {
            await dbContext.AddProviderGpuAsync(
                new ProviderGpu
                {
                    ComputeProviderId = provider.Id,
                    GpuId = gpu.GpuId,
                    Name = gpu.Name,
                    GpuType = gpu.GpuType,
                    MemoryGb = gpu.MemoryGb,
                    IsAvailable = gpu.IsAvailable,
                    SyncedAt = now,
                },
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task<string> GetDecryptedApiKeyAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        if (provider.Credential is null)
        {
            throw new InvalidOperationException("Provider credentials are not configured.");
        }

        return Task.FromResult(GetDecryptedApiKey(provider));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderRegionInfo>> ListRegionsAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await computeProviderFactory.GetProvider(provider.ProviderType)
            .ListRegionsAsync(apiKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderGpuInfo>> ListGpusAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await computeProviderFactory.GetProvider(provider.ProviderType)
            .ListGpusAsync(apiKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderTemplateInfo>> ListTemplatesAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await computeProviderFactory.GetProvider(provider.ProviderType)
            .ListTemplatesAsync(apiKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProviderAccountInfo> GetAccountInfoAsync(
        ComputeProvider provider,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await computeProviderFactory.GetProvider(provider.ProviderType)
            .GetAccountInfoAsync(apiKey, cancellationToken);
    }

    private string GetDecryptedApiKey(ComputeProvider provider)
    {
        if (provider.Credential is null || string.IsNullOrWhiteSpace(provider.Credential.EncryptedApiKey))
        {
            throw new InvalidOperationException("Provider credentials are not configured.");
        }

        return encryptionService.Decrypt(provider.Credential.EncryptedApiKey);
    }
}
