using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// License activation, validation, and issuance.
/// </summary>
public sealed class LicenseService : ILicenseService
{
    private readonly IApplicationDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseService"/> class.
    /// </summary>
    public LicenseService(IApplicationDbContext db) => this.db = db;

    /// <inheritdoc />
    public async Task<LicenseInfo> ActivateAsync(
        Guid organizationId,
        string licenseKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(licenseKey), "License key is required."),
            ]);
        }

        var normalized = licenseKey.Trim().ToUpperInvariant();
        var hash = HashKey(normalized);
        var license = await db.ProductLicenses
            .FirstOrDefaultAsync(l => l.LicenseKeyHash == hash, cancellationToken)
            ?? throw new NotFoundException("Product license", licenseKey);

        if (license.OrganizationId.HasValue && license.OrganizationId.Value != organizationId)
        {
            throw new ForbiddenException("This license is already activated for another organization.");
        }

        if (license.ExpiresAt.HasValue && license.ExpiresAt.Value < DateTime.UtcNow)
        {
            license.IsValid = false;
            await db.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException("This license has expired.");
        }

        license.OrganizationId = organizationId;
        license.IsActivated = true;
        license.ActivatedAt = DateTime.UtcNow;
        license.LastValidatedAt = DateTime.UtcNow;
        license.IsValid = true;
        license.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Map(license);
    }

    /// <inheritdoc />
    public async Task<LicenseInfo> ValidateAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var license = await db.ProductLicenses
            .Where(l => l.OrganizationId == organizationId && l.IsActivated)
            .OrderByDescending(l => l.ActivatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (license is null)
        {
            return await IssueCommunityAsync(organizationId, cancellationToken);
        }

        if (license.ExpiresAt.HasValue && license.ExpiresAt.Value < DateTime.UtcNow)
        {
            license.IsValid = false;
            license.LastValidatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return Map(license);
        }

        license.LastValidatedAt = DateTime.UtcNow;
        license.IsValid = true;
        await db.SaveChangesAsync(cancellationToken);
        return Map(license);
    }

    /// <inheritdoc />
    public async Task<IssuedLicense> IssueAsync(IssueLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var editionToken = request.Edition switch
        {
            LicenseEdition.Community => "COMMUNITY",
            LicenseEdition.Professional => "PRO",
            LicenseEdition.Enterprise => "ENTERPRISE",
            _ => "PRO",
        };

        var rawKey = $"PP-{editionToken}-{Guid.NewGuid():N}".ToUpperInvariant();
        var prefix = rawKey.Length <= 24 ? rawKey : rawKey[..24];
        var license = new ProductLicense
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            LicenseKeyHash = HashKey(rawKey),
            LicenseKeyPrefix = prefix,
            Edition = request.Edition,
            DeploymentMode = request.DeploymentMode,
            IsActivated = request.OrganizationId.HasValue,
            ActivatedAt = request.OrganizationId.HasValue ? DateTime.UtcNow : null,
            ExpiresAt = request.ExpiresAt,
            MaxSeats = Math.Max(1, request.MaxSeats),
            IsValid = true,
            LastValidatedAt = request.OrganizationId.HasValue ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
        };

        if (request.DeploymentMode == LicenseDeploymentMode.Offline)
        {
            license.EncryptedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawKey));
        }

        await db.AddProductLicenseAsync(license, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new IssuedLicense
        {
            Info = Map(license),
            LicenseKey = rawKey,
        };
    }

    private async Task<LicenseInfo> IssueCommunityAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var existingCommunity = await db.ProductLicenses
            .Where(l => l.OrganizationId == organizationId && l.Edition == LicenseEdition.Community)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingCommunity is not null)
        {
            existingCommunity.IsActivated = true;
            existingCommunity.IsValid = true;
            existingCommunity.ActivatedAt ??= DateTime.UtcNow;
            existingCommunity.LastValidatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return Map(existingCommunity);
        }

        var issued = await IssueAsync(
            new IssueLicenseRequest
            {
                OrganizationId = organizationId,
                Edition = LicenseEdition.Community,
                DeploymentMode = LicenseDeploymentMode.Online,
                MaxSeats = 1,
            },
            cancellationToken);

        return issued.Info;
    }

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes);
    }

    private static LicenseInfo Map(ProductLicense license) =>
        new()
        {
            Id = license.Id,
            LicenseKeyPrefix = license.LicenseKeyPrefix,
            Edition = license.Edition,
            DeploymentMode = license.DeploymentMode,
            IsActivated = license.IsActivated,
            IsValid = license.IsValid,
            ExpiresAt = license.ExpiresAt,
            MaxSeats = license.MaxSeats,
            LastValidatedAt = license.LastValidatedAt,
        };
}
