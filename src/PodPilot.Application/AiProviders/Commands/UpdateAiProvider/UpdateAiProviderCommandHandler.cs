using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.UpdateAiProvider;

/// <summary>
/// Handles AI provider updates.
/// </summary>
public sealed class UpdateAiProviderCommandHandler : IRequestHandler<UpdateAiProviderCommand, AiProviderResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderFactory providerFactory;
    private readonly IAiProviderRegistry providerRegistry;
    private readonly IAiProviderService aiProviderService;
    private readonly IEncryptionService encryptionService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;
    private readonly IAiProviderNotificationService notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAiProviderCommandHandler"/> class.
    /// </summary>
    public UpdateAiProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAiProviderFactory providerFactory,
        IAiProviderRegistry providerRegistry,
        IAiProviderService aiProviderService,
        IEncryptionService encryptionService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService,
        IAiProviderNotificationService notificationService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.providerFactory = providerFactory;
        this.providerRegistry = providerRegistry;
        this.aiProviderService = aiProviderService;
        this.encryptionService = encryptionService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<AiProviderResponse> Handle(UpdateAiProviderCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.AiProviderUpdate,
            cancellationToken);

        var provider = await AiProviderAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken,
            includeCredential: true);

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.AiInferenceProviders.AnyAsync(
            p => p.OrganizationId == organizationId && p.Name == normalizedName && p.Id != provider.Id,
            cancellationToken);
        if (nameExists)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Name),
                    "An AI provider with this name already exists."),
            ]);
        }

        var metadata = providerRegistry.GetMetadata(provider.ProviderKind);
        if (metadata.RequiresBaseUrl && string.IsNullOrWhiteSpace(request.BaseUrl) && string.IsNullOrWhiteSpace(provider.BaseUrl))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.BaseUrl),
                    "Base URL is required for this provider kind."),
            ]);
        }

        var now = dateTimeService.UtcNow;
        provider.Name = normalizedName;
        provider.DisplayName = request.DisplayName.Trim();
        provider.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        provider.BaseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? provider.BaseUrl : request.BaseUrl.Trim();
        provider.DeploymentName = string.IsNullOrWhiteSpace(request.DeploymentName) ? null : request.DeploymentName.Trim();
        provider.ApiVersion = string.IsNullOrWhiteSpace(request.ApiVersion) ? null : request.ApiVersion.Trim();
        provider.IsEnabled = request.IsEnabled;
        provider.Priority = request.Priority;
        provider.UpdatedAt = now;
        provider.UpdatedBy = userId.ToString();

        string? plainKey = null;
        if (!string.IsNullOrWhiteSpace(request.ApiKey))
        {
            plainKey = request.ApiKey.Trim();
            if (provider.Credential is null)
            {
                provider.Credential = new AiProviderCredential
                {
                    AiProviderId = provider.Id,
                    CreatedAt = now,
                    CreatedBy = userId.ToString(),
                };
            }

            provider.Credential.EncryptedApiKey = encryptionService.Encrypt(plainKey);
            provider.Credential.UpdatedAt = now;
            provider.Credential.UpdatedBy = userId.ToString();
        }
        else if (provider.Credential is not null)
        {
            plainKey = encryptionService.Decrypt(provider.Credential.EncryptedApiKey);
        }

        if (plainKey is not null)
        {
            var connection = new AiProviderConnection
            {
                OrganizationId = organizationId,
                ProviderId = provider.Id,
                ProviderKind = provider.ProviderKind,
                ApiKey = plainKey,
                BaseUrl = string.IsNullOrWhiteSpace(provider.BaseUrl) ? metadata.DefaultBaseUrl : provider.BaseUrl,
                DeploymentName = provider.DeploymentName,
                ApiVersion = provider.ApiVersion,
            };

            var validation = await providerFactory.GetProvider(provider.ProviderKind)
                .ValidateCredentialsAsync(connection, cancellationToken);
            if (!validation.IsValid)
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.ApiKey),
                        validation.Message ?? "AI provider credentials are invalid."),
                ]);
            }

            provider.IsValidated = true;
            provider.LastValidatedAt = now;
            await aiProviderService.SyncModelsAsync(provider, plainKey, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(AiInferenceProvider),
            provider.Id.ToString(),
            $"AI provider '{provider.DisplayName}' updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyModelCatalogUpdatedAsync(organizationId, provider.Id, cancellationToken);
        return AiProviderMapper.ToResponse(provider);
    }
}
