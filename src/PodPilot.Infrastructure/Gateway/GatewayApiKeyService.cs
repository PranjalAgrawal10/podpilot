using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Manages gateway API key lifecycle and validation.
/// </summary>
public sealed class GatewayApiKeyService : IGatewayApiKeyService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayApiKeyService"/> class.
    /// </summary>
    public GatewayApiKeyService(
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<GatewayAuthContext?> ValidateKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < ApplicationConstants.GatewayApiKeyPrefixLength)
        {
            return null;
        }

        var prefix = apiKey[..ApplicationConstants.GatewayApiKeyPrefixLength];
        var hash = HashKey(apiKey);

        var entity = await dbContext.GatewayApiKeys
            .Where(k => k.KeyPrefix == prefix && k.KeyHash == hash && !k.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var now = dateTimeService.UtcNow;
        if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value <= now)
        {
            return null;
        }

        return new GatewayAuthContext
        {
            ApiKeyId = entity.Id,
            OrganizationId = entity.OrganizationId,
            UserId = entity.UserId,
            RateLimitPerMinute = entity.RateLimitPerMinute,
            RateLimitPerDay = entity.RateLimitPerDay,
        };
    }

    /// <inheritdoc />
    public async Task<(GatewayApiKey Entity, string PlaintextKey)> CreateKeyAsync(
        Guid organizationId,
        Guid? userId,
        string name,
        DateTime? expiresAt,
        int rateLimitPerMinute,
        int rateLimitPerDay,
        CancellationToken cancellationToken = default)
    {
        var plaintextKey = GenerateKey();
        var entity = new GatewayApiKey
        {
            OrganizationId = organizationId,
            UserId = userId,
            KeyType = userId.HasValue ? GatewayApiKeyType.Personal : GatewayApiKeyType.Organization,
            Name = name.Trim(),
            KeyPrefix = plaintextKey[..ApplicationConstants.GatewayApiKeyPrefixLength],
            KeyHash = HashKey(plaintextKey),
            ExpiresAt = expiresAt,
            RateLimitPerMinute = rateLimitPerMinute,
            RateLimitPerDay = rateLimitPerDay,
            CreatedAt = dateTimeService.UtcNow,
            UpdatedAt = dateTimeService.UtcNow,
        };

        await dbContext.AddGatewayApiKeyAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (entity, plaintextKey);
    }

    /// <inheritdoc />
    public async Task RevokeKeyAsync(
        Guid keyId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.GatewayApiKeys
            .Where(k => k.Id == keyId && k.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Gateway API key was not found.");

        entity.IsRevoked = true;
        entity.RevokedAt = dateTimeService.UtcNow;
        entity.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(GatewayApiKey Entity, string PlaintextKey)> RotateKeyAsync(
        Guid keyId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.GatewayApiKeys
            .Where(k => k.Id == keyId && k.OrganizationId == organizationId && !k.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Gateway API key was not found.");

        var plaintextKey = GenerateKey();
        entity.KeyPrefix = plaintextKey[..ApplicationConstants.GatewayApiKeyPrefixLength];
        entity.KeyHash = HashKey(plaintextKey);
        entity.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return (entity, plaintextKey);
    }

    private static string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var key = $"sk-{token}";
        while (key.Length < ApplicationConstants.GatewayApiKeyLength)
        {
            key += Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
        }

        return key[..ApplicationConstants.GatewayApiKeyLength];
    }

    private static string HashKey(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(bytes);
    }
}
