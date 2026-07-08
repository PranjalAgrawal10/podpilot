using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Providers.Commands.UpdateProvider;

/// <summary>
/// Handles compute provider updates.
/// </summary>
public sealed class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, ProviderResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IProviderService providerService;
    private readonly IEncryptionService encryptionService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProviderCommandHandler"/> class.
    /// </summary>
    public UpdateProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IProviderService providerService,
        IEncryptionService encryptionService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.providerService = providerService;
        this.encryptionService = encryptionService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<ProviderResponse> Handle(
        UpdateProviderCommand request,
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

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim();
            var nameExists = await dbContext.ComputeProviders.AnyAsync(
                p => p.OrganizationId == organizationId
                     && p.Name == normalizedName
                     && p.Id != provider.Id,
                cancellationToken);

            if (nameExists)
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.Name),
                        "A provider with this name already exists."),
                ]);
            }

            provider.Name = normalizedName;
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            provider.DisplayName = request.DisplayName.Trim();
        }

        if (request.Description is not null)
        {
            provider.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        }

        if (request.DefaultRegion is not null)
        {
            provider.DefaultRegion = string.IsNullOrWhiteSpace(request.DefaultRegion)
                ? null
                : request.DefaultRegion.Trim();
        }

        if (request.IsEnabled.HasValue)
        {
            provider.IsEnabled = request.IsEnabled.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.ApiKey))
        {
            var apiKey = request.ApiKey.Trim();
            var validation = await providerService.ValidateCredentialsAsync(
                provider.ProviderType,
                apiKey,
                cancellationToken);

            if (!validation.IsValid)
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.ApiKey),
                        validation.ErrorMessage ?? "Provider credentials are invalid."),
                ]);
            }

            if (provider.Credential is null)
            {
                provider.Credential = new ProviderCredential
                {
                    ComputeProviderId = provider.Id,
                    CreatedAt = dateTimeService.UtcNow,
                    CreatedBy = userId.ToString(),
                };

                await dbContext.AddProviderCredentialAsync(provider.Credential, cancellationToken);
            }

            provider.Credential.EncryptedApiKey = encryptionService.Encrypt(apiKey);
            provider.Credential.UpdatedAt = dateTimeService.UtcNow;
            provider.Credential.UpdatedBy = userId.ToString();
            provider.IsValidated = true;
            provider.LastValidatedAt = dateTimeService.UtcNow;
            await providerService.SyncCatalogAsync(provider, apiKey, cancellationToken);
        }

        provider.UpdatedAt = dateTimeService.UtcNow;
        provider.UpdatedBy = userId.ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(ComputeProvider),
            provider.Id.ToString(),
            $"Provider '{provider.DisplayName}' updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return ProviderMapper.ToResponse(provider);
    }
}
