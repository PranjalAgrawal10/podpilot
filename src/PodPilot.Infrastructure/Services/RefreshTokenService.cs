using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Configuration;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Refresh token management with rotation support.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly JwtSettings jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenService"/> class.
    /// </summary>
    public RefreshTokenService(
        ApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        IOptions<JwtSettings> jwtSettings)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public async Task<(RefreshToken Entity, string Token)> GenerateRefreshTokenAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = dateTimeService.UtcNow,
            ExpiresAt = dateTimeService.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays),
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (refreshToken, token);
    }

    /// <inheritdoc />
    public async Task<(RefreshToken Entity, string Token)> RotateRefreshTokenAsync(
        string token,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            if (existingToken is not null && existingToken.IsRevoked)
            {
                await RevokeDescendantTokensAsync(existingToken, cancellationToken);
            }

            throw new UnauthorizedException("Invalid refresh token.");
        }

        var newTokenValue = GenerateSecureToken();
        var newRefreshToken = new RefreshToken
        {
            Token = newTokenValue,
            UserId = existingToken.UserId,
            CreatedAt = dateTimeService.UtcNow,
            ExpiresAt = dateTimeService.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays),
        };

        existingToken.RevokedAt = dateTimeService.UtcNow;
        existingToken.ReplacedByToken = newTokenValue;
        existingToken.ReasonRevoked = "Rotated";

        await dbContext.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (newRefreshToken, newTokenValue);
    }

    /// <inheritdoc />
    public async Task RevokeRefreshTokenAsync(
        string token,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return;
        }

        refreshToken.RevokedAt = dateTimeService.UtcNow;
        refreshToken.ReasonRevoked = reason;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = dateTimeService.UtcNow;
            token.ReasonRevoked = reason;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeDescendantTokensAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token.ReplacedByToken))
        {
            return;
        }

        var descendant = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token.ReplacedByToken, cancellationToken);

        if (descendant is null)
        {
            return;
        }

        if (descendant.IsActive)
        {
            descendant.RevokedAt = dateTimeService.UtcNow;
            descendant.ReasonRevoked = "Revoked due to token reuse";
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await RevokeDescendantTokensAsync(descendant, cancellationToken);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
