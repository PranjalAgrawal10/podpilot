using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Security.Secrets;

/// <summary>
/// Stores secrets encrypted in the application database.
/// </summary>
public sealed class LocalEncryptedSecretProvider : ISecretProvider
{
    private readonly IEncryptionService encryptionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalEncryptedSecretProvider"/> class.
    /// </summary>
    public LocalEncryptedSecretProvider(IEncryptionService encryptionService) =>
        this.encryptionService = encryptionService;

    /// <inheritdoc />
    public SecretBackendKind BackendKind => SecretBackendKind.LocalEncrypted;

    /// <inheritdoc />
    public Task<string> StoreAsync(SecretStoreRequest request, CancellationToken cancellationToken = default)
    {
        var locator = string.IsNullOrWhiteSpace(request.ExistingLocator)
            ? $"local:{request.OrganizationId:N}:{Guid.NewGuid():N}"
            : request.ExistingLocator;
        return Task.FromResult(locator);
    }

    /// <inheritdoc />
    public Task<string?> GetValueAsync(string backendLocator, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);

    /// <inheritdoc />
    public Task RotateAsync(string backendLocator, string newPlaintext, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task DeleteAsync(string backendLocator, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <summary>Encrypts a plaintext value for local storage.</summary>
    public string EncryptValue(string plaintext) => encryptionService.Encrypt(plaintext);

    /// <summary>Decrypts a locally stored value.</summary>
    public string DecryptValue(string ciphertext) => encryptionService.Decrypt(ciphertext);
}

/// <summary>
/// Azure Key Vault secret provider (metadata + local fallback in Testing).
/// </summary>
public sealed class AzureKeyVaultSecretProvider : ExternalSecretProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultSecretProvider"/> class.
    /// </summary>
    public AzureKeyVaultSecretProvider(
        IConfiguration configuration,
        IHostEnvironment environment,
        IEncryptionService encryptionService)
        : base(configuration, environment, encryptionService, "Security:AzureKeyVault:VaultUri", "azure-kv")
    {
    }

    /// <inheritdoc />
    public override SecretBackendKind BackendKind => SecretBackendKind.AzureKeyVault;
}

/// <summary>
/// AWS Secrets Manager provider (metadata + local fallback in Testing).
/// </summary>
public sealed class AwsSecretsManagerSecretProvider : ExternalSecretProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AwsSecretsManagerSecretProvider"/> class.
    /// </summary>
    public AwsSecretsManagerSecretProvider(
        IConfiguration configuration,
        IHostEnvironment environment,
        IEncryptionService encryptionService)
        : base(configuration, environment, encryptionService, "Security:AwsSecretsManager:Region", "aws-sm")
    {
    }

    /// <inheritdoc />
    public override SecretBackendKind BackendKind => SecretBackendKind.AwsSecretsManager;
}

/// <summary>
/// HashiCorp Vault provider (metadata + local fallback in Testing).
/// </summary>
public sealed class HashiCorpVaultSecretProvider : ExternalSecretProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HashiCorpVaultSecretProvider"/> class.
    /// </summary>
    public HashiCorpVaultSecretProvider(
        IConfiguration configuration,
        IHostEnvironment environment,
        IEncryptionService encryptionService)
        : base(configuration, environment, encryptionService, "Security:HashiCorpVault:Address", "hcv")
    {
    }

    /// <inheritdoc />
    public override SecretBackendKind BackendKind => SecretBackendKind.HashiCorpVault;
}

/// <summary>
/// Shared external backend behavior: store locator metadata; decrypt locally only in Testing.
/// </summary>
public abstract class ExternalSecretProviderBase : ISecretProvider
{
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment environment;
    private readonly IEncryptionService encryptionService;
    private readonly string configKey;
    private readonly string locatorPrefix;
    private readonly Dictionary<string, string> testingValues = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalSecretProviderBase"/> class.
    /// </summary>
    protected ExternalSecretProviderBase(
        IConfiguration configuration,
        IHostEnvironment environment,
        IEncryptionService encryptionService,
        string configKey,
        string locatorPrefix)
    {
        this.configuration = configuration;
        this.environment = environment;
        this.encryptionService = encryptionService;
        this.configKey = configKey;
        this.locatorPrefix = locatorPrefix;
    }

    /// <inheritdoc />
    public abstract SecretBackendKind BackendKind { get; }

    private bool IsConfigured => !string.IsNullOrWhiteSpace(configuration[configKey]);

    private bool UseLocalFallback => environment.IsEnvironment("Testing") || !IsConfigured;

    /// <inheritdoc />
    public Task<string> StoreAsync(SecretStoreRequest request, CancellationToken cancellationToken = default)
    {
        var locator = string.IsNullOrWhiteSpace(request.ExistingLocator)
            ? $"{locatorPrefix}:{request.OrganizationId:N}:{request.Name}"
            : request.ExistingLocator;

        if (UseLocalFallback)
        {
            testingValues[locator] = encryptionService.Encrypt(request.Plaintext);
        }

        return Task.FromResult(locator);
    }

    /// <inheritdoc />
    public Task<string?> GetValueAsync(string backendLocator, CancellationToken cancellationToken = default)
    {
        if (UseLocalFallback && testingValues.TryGetValue(backendLocator, out var encrypted))
        {
            return Task.FromResult<string?>(encryptionService.Decrypt(encrypted));
        }

        if (!IsConfigured)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(BackendKind),
                    $"{BackendKind} is not configured. Set '{configKey}' or use LocalEncrypted."),
            ]);
        }

        throw new ValidationException(
        [
            new ValidationFailure(
                nameof(BackendKind),
                $"{BackendKind} retrieval requires a live vault integration that is not available in this build."),
        ]);
    }

    /// <inheritdoc />
    public async Task RotateAsync(string backendLocator, string newPlaintext, CancellationToken cancellationToken = default)
    {
        if (UseLocalFallback)
        {
            testingValues[backendLocator] = encryptionService.Encrypt(newPlaintext);
            return;
        }

        _ = await GetValueAsync(backendLocator, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string backendLocator, CancellationToken cancellationToken = default)
    {
        testingValues.Remove(backendLocator);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Resolves secret providers by backend kind.
/// </summary>
public sealed class SecretProviderFactory : ISecretProviderFactory
{
    private readonly IReadOnlyDictionary<SecretBackendKind, ISecretProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretProviderFactory"/> class.
    /// </summary>
    public SecretProviderFactory(IEnumerable<ISecretProvider> providers) =>
        this.providers = providers.ToDictionary(p => p.BackendKind);

    /// <inheritdoc />
    public ISecretProvider GetProvider(SecretBackendKind backendKind) =>
        providers.TryGetValue(backendKind, out var provider)
            ? provider
            : throw new ValidationException($"Unsupported secret backend '{backendKind}'.");
}

/// <summary>
/// Organization secret catalog manager.
/// </summary>
public sealed class SecretManager : ISecretManager
{
    private readonly IApplicationDbContext dbContext;
    private readonly ISecretProviderFactory providerFactory;
    private readonly IDateTimeService dateTimeService;
    private readonly LocalEncryptedSecretProvider localProvider;
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManager"/> class.
    /// </summary>
    public SecretManager(
        IApplicationDbContext dbContext,
        ISecretProviderFactory providerFactory,
        IDateTimeService dateTimeService,
        LocalEncryptedSecretProvider localProvider,
        IHostEnvironment environment)
    {
        this.dbContext = dbContext;
        this.providerFactory = providerFactory;
        this.dateTimeService = dateTimeService;
        this.localProvider = localProvider;
        this.environment = environment;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(
        Guid organizationId,
        CreateSecretRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Plaintext))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Name), "Name and value are required."),
            ]);
        }

        var duplicate = await dbContext.SecretReferences.AnyAsync(
            s => s.OrganizationId == organizationId && s.Name == request.Name.Trim(),
            cancellationToken);
        if (duplicate)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Name), "A secret with this name already exists."),
            ]);
        }

        var provider = providerFactory.GetProvider(request.BackendKind);
        var locator = await provider.StoreAsync(
            new SecretStoreRequest
            {
                OrganizationId = organizationId,
                Name = request.Name.Trim(),
                Plaintext = request.Plaintext,
            },
            cancellationToken);

        var now = dateTimeService.UtcNow;
        var persistLocally = request.BackendKind == SecretBackendKind.LocalEncrypted ||
                             environment.IsEnvironment("Testing");
        var entity = new Domain.Entities.SecretReference
        {
            OrganizationId = organizationId,
            Name = request.Name.Trim(),
            SecretKind = request.SecretKind,
            BackendKind = request.BackendKind,
            BackendLocator = locator,
            EncryptedValue = persistLocally
                ? localProvider.EncryptValue(request.Plaintext)
                : null,
            ExpiresAt = request.ExpiresAt,
            LastRotatedAt = now,
            IsEnabled = true,
            Version = 1,
            CreatedAt = now,
        };

        await dbContext.AddSecretReferenceAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        Guid organizationId,
        Guid secretId,
        UpdateSecretRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecretReferences
            .FirstOrDefaultAsync(s => s.Id == secretId && s.OrganizationId == organizationId, cancellationToken)
            ?? throw new NotFoundException("Secret", secretId);

        if (!string.IsNullOrWhiteSpace(request.Name) &&
            !string.Equals(request.Name.Trim(), entity.Name, StringComparison.Ordinal))
        {
            var duplicate = await dbContext.SecretReferences.AnyAsync(
                s => s.OrganizationId == organizationId &&
                     s.Name == request.Name.Trim() &&
                     s.Id != secretId,
                cancellationToken);
            if (duplicate)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(request.Name), "A secret with this name already exists."),
                ]);
            }

            entity.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Plaintext))
        {
            var provider = providerFactory.GetProvider(entity.BackendKind);
            entity.BackendLocator = await provider.StoreAsync(
                new SecretStoreRequest
                {
                    OrganizationId = organizationId,
                    Name = entity.Name,
                    Plaintext = request.Plaintext,
                    ExistingLocator = entity.BackendLocator,
                },
                cancellationToken);
            await provider.RotateAsync(entity.BackendLocator, request.Plaintext, cancellationToken);
            if (entity.BackendKind == SecretBackendKind.LocalEncrypted || environment.IsEnvironment("Testing"))
            {
                entity.EncryptedValue = localProvider.EncryptValue(request.Plaintext);
            }

            entity.Version += 1;
            entity.LastRotatedAt = dateTimeService.UtcNow;
        }

        if (request.ExpiresAt.HasValue)
        {
            entity.ExpiresAt = request.ExpiresAt;
        }

        if (request.IsEnabled.HasValue)
        {
            entity.IsEnabled = request.IsEnabled.Value;
        }

        entity.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid organizationId, Guid secretId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecretReferences
            .FirstOrDefaultAsync(s => s.Id == secretId && s.OrganizationId == organizationId, cancellationToken)
            ?? throw new NotFoundException("Secret", secretId);

        var provider = providerFactory.GetProvider(entity.BackendKind);
        await provider.DeleteAsync(entity.BackendLocator, cancellationToken);
        await dbContext.RemoveSecretReferenceAsync(entity.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string?> ResolveAsync(
        Guid organizationId,
        Guid secretId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecretReferences
            .FirstOrDefaultAsync(
                s => s.Id == secretId && s.OrganizationId == organizationId && s.IsEnabled,
                cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.LastAccessedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.EncryptedValue))
        {
            return localProvider.DecryptValue(entity.EncryptedValue);
        }

        var provider = providerFactory.GetProvider(entity.BackendKind);
        return await provider.GetValueAsync(entity.BackendLocator, cancellationToken);
    }
}
