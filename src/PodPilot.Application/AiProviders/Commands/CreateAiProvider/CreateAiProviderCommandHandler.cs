using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.CreateAiProvider;

/// <summary>
/// Handles AI provider creation.
/// </summary>
public sealed class CreateAiProviderCommandHandler : IRequestHandler<CreateAiProviderCommand, AiProviderResponse>
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
    /// Initializes a new instance of the <see cref="CreateAiProviderCommandHandler"/> class.
    /// </summary>
    public CreateAiProviderCommandHandler(
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
    public async Task<AiProviderResponse> Handle(CreateAiProviderCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.AiProviderCreate,
            cancellationToken);

        var metadata = providerRegistry.GetMetadata(request.ProviderKind);
        if (metadata.RequiresBaseUrl && string.IsNullOrWhiteSpace(request.BaseUrl))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.BaseUrl),
                    "Base URL is required for this provider kind."),
            ]);
        }

        if (metadata.RequiresApiKey && string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ApiKey),
                    "API key is required for this provider kind."),
            ]);
        }

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.AiInferenceProviders.AnyAsync(
            p => p.OrganizationId == organizationId && p.Name == normalizedName,
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

        var apiKey = request.ApiKey?.Trim() ?? string.Empty;
        var baseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? metadata.DefaultBaseUrl : request.BaseUrl.Trim();
        var connection = new AiProviderConnection
        {
            OrganizationId = organizationId,
            ProviderId = Guid.Empty,
            ProviderKind = request.ProviderKind,
            ApiKey = apiKey,
            BaseUrl = baseUrl,
            DeploymentName = string.IsNullOrWhiteSpace(request.DeploymentName) ? null : request.DeploymentName.Trim(),
            ApiVersion = string.IsNullOrWhiteSpace(request.ApiVersion) ? null : request.ApiVersion.Trim(),
        };

        var validation = await providerFactory.GetProvider(request.ProviderKind)
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

        var now = dateTimeService.UtcNow;
        var provider = new AiInferenceProvider
        {
            OrganizationId = organizationId,
            Name = normalizedName,
            DisplayName = request.DisplayName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ProviderKind = request.ProviderKind,
            BaseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? null : request.BaseUrl.Trim(),
            DeploymentName = connection.DeploymentName,
            ApiVersion = connection.ApiVersion,
            IsEnabled = request.IsEnabled,
            IsValidated = true,
            LastValidatedAt = now,
            Priority = request.Priority,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        provider.Credential = new AiProviderCredential
        {
            AiProviderId = provider.Id,
            EncryptedApiKey = encryptionService.Encrypt(apiKey),
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        provider.Health = new AiProviderHealth
        {
            AiProviderId = provider.Id,
            Status = AiProviderHealthState.Unknown,
            LastCheckedAt = now,
        };

        await dbContext.AddAiInferenceProviderAsync(provider, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await aiProviderService.SyncModelsAsync(provider, apiKey, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(AiInferenceProvider),
            provider.Id.ToString(),
            $"AI provider '{provider.DisplayName}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyProviderConnectedAsync(organizationId, provider.Id, cancellationToken);
        await notificationService.NotifyModelCatalogUpdatedAsync(organizationId, provider.Id, cancellationToken);

        return AiProviderMapper.ToResponse(provider);
    }
}
