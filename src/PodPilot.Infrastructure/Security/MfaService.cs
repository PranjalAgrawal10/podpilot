using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// TOTP MFA enrollment and verification.
/// </summary>
public sealed class MfaService : IMfaService
{
    private const string Issuer = "PodPilot";

    private readonly IApplicationDbContext dbContext;
    private readonly IEncryptionService encryptionService;
    private readonly IIdentityService identityService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MfaService"/> class.
    /// </summary>
    public MfaService(
        IApplicationDbContext dbContext,
        IEncryptionService encryptionService,
        IIdentityService identityService,
        IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.encryptionService = encryptionService;
        this.identityService = identityService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<MfaEnrollmentResult> BeginEnrollmentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await identityService.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var secret = TotpService.GenerateSecret();
        var enrollment = await dbContext.UserMfaEnrollments
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (enrollment is null)
        {
            enrollment = new UserMfaEnrollment
            {
                UserId = userId,
                CreatedAt = dateTimeService.UtcNow,
            };
            await dbContext.AddUserMfaEnrollmentAsync(enrollment, cancellationToken);
        }

        enrollment.EncryptedTotpSecret = encryptionService.Encrypt(secret);
        enrollment.IsEnabled = false;
        enrollment.EnabledAt = null;
        enrollment.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new MfaEnrollmentResult
        {
            SharedSecret = secret,
            OtpAuthUri = TotpService.BuildOtpAuthUri(Issuer, user.Email, secret),
        };
    }

    /// <inheritdoc />
    public async Task ConfirmEnrollmentAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var enrollment = await dbContext.UserMfaEnrollments
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken)
            ?? throw new ValidationException("MFA enrollment has not been started.");

        if (string.IsNullOrWhiteSpace(enrollment.EncryptedTotpSecret))
        {
            throw new ValidationException("MFA enrollment has not been started.");
        }

        var secret = encryptionService.Decrypt(enrollment.EncryptedTotpSecret);
        if (!TotpService.ValidateCode(secret, code))
        {
            throw new ValidationException("Invalid MFA code.");
        }

        enrollment.IsEnabled = true;
        enrollment.EnabledAt = dateTimeService.UtcNow;
        enrollment.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> VerifyAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var enrollment = await dbContext.UserMfaEnrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId && e.IsEnabled, cancellationToken);
        if (enrollment?.EncryptedTotpSecret is null)
        {
            return false;
        }

        var secret = encryptionService.Decrypt(enrollment.EncryptedTotpSecret);
        return TotpService.ValidateCode(secret, code);
    }

    /// <inheritdoc />
    public async Task DisableAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var enrollment = await dbContext.UserMfaEnrollments
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
        if (enrollment is null)
        {
            return;
        }

        enrollment.IsEnabled = false;
        enrollment.EnabledAt = null;
        enrollment.EncryptedTotpSecret = null;
        enrollment.EncryptedRecoveryCodesJson = null;
        enrollment.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.UserMfaEnrollments.AsNoTracking()
            .AnyAsync(e => e.UserId == userId && e.IsEnabled, cancellationToken);
}
