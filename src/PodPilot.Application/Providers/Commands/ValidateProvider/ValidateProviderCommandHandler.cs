using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Compute;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Providers.Commands.ValidateProvider;

/// <summary>
/// Handles provider credential validation.
/// </summary>
public sealed class ValidateProviderCommandHandler : IRequestHandler<ValidateProviderCommand, ProviderValidationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IProviderService providerService;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateProviderCommandHandler"/> class.
    /// </summary>
    public ValidateProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IProviderService providerService,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.providerService = providerService;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<ProviderValidationResponse> Handle(
        ValidateProviderCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderUpdate,
            cancellationToken);

        var provider = await ProviderAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken,
            includeCredential: true);

        ProviderValidationResult validation;
        if (!string.IsNullOrWhiteSpace(request.ApiKey))
        {
            validation = await providerService.ValidateCredentialsAsync(
                provider.ProviderType,
                request.ApiKey.Trim(),
                cancellationToken);
        }
        else
        {
            validation = await providerService.ValidateProviderAsync(provider, cancellationToken);
        }

        if (validation.IsValid)
        {
            provider.IsValidated = true;
            provider.LastValidatedAt = dateTimeService.UtcNow;
            provider.UpdatedAt = dateTimeService.UtcNow;
            provider.UpdatedBy = userId.ToString();

            if (!string.IsNullOrWhiteSpace(request.ApiKey) && provider.Credential is not null)
            {
                provider.Credential.EncryptedApiKey = encryptionService.Encrypt(request.ApiKey.Trim());
                provider.Credential.UpdatedAt = dateTimeService.UtcNow;
                provider.Credential.UpdatedBy = userId.ToString();
                await providerService.SyncCatalogAsync(provider, request.ApiKey.Trim(), cancellationToken);
            }

            await providerService.CheckAndPersistHealthAsync(provider, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ProviderValidationMapper.ToResponse(validation);
    }
}
