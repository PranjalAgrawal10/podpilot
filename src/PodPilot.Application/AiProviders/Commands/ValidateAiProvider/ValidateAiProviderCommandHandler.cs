using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.ValidateAiProvider;

/// <summary>
/// Handles AI provider credential validation.
/// </summary>
public sealed class ValidateAiProviderCommandHandler : IRequestHandler<ValidateAiProviderCommand, AiProviderValidationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderFactory providerFactory;
    private readonly IAiProviderRegistry providerRegistry;
    private readonly IAiProviderService aiProviderService;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateAiProviderCommandHandler"/> class.
    /// </summary>
    public ValidateAiProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAiProviderFactory providerFactory,
        IAiProviderRegistry providerRegistry,
        IAiProviderService aiProviderService,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.providerFactory = providerFactory;
        this.providerRegistry = providerRegistry;
        this.aiProviderService = aiProviderService;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<AiProviderValidationResponse> Handle(
        ValidateAiProviderCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            request.ProviderId.HasValue ? PermissionNames.AiProviderUpdate : PermissionNames.AiProviderCreate,
            cancellationToken);

        AiProviderConnection connection;
        if (request.ProviderId.HasValue)
        {
            var provider = await AiProviderAccess.GetProviderAsync(
                dbContext,
                request.ProviderId.Value,
                organizationId,
                cancellationToken,
                includeCredential: true);

            if (!string.IsNullOrWhiteSpace(request.ApiKey) && provider.Credential is not null)
            {
                provider.Credential.EncryptedApiKey = encryptionService.Encrypt(request.ApiKey.Trim());
            }

            connection = await aiProviderService.CreateConnectionAsync(provider, cancellationToken);
            if (!string.IsNullOrWhiteSpace(request.BaseUrl))
            {
                connection = new AiProviderConnection
                {
                    OrganizationId = connection.OrganizationId,
                    ProviderId = connection.ProviderId,
                    ProviderKind = connection.ProviderKind,
                    ApiKey = string.IsNullOrWhiteSpace(request.ApiKey) ? connection.ApiKey : request.ApiKey.Trim(),
                    BaseUrl = request.BaseUrl.Trim(),
                    DeploymentName = request.DeploymentName ?? connection.DeploymentName,
                    ApiVersion = request.ApiVersion ?? connection.ApiVersion,
                };
            }

            var result = await providerFactory.GetProvider(provider.ProviderKind)
                .ValidateCredentialsAsync(connection, cancellationToken);
            if (result.IsValid)
            {
                provider.IsValidated = true;
                provider.LastValidatedAt = dateTimeService.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new AiProviderValidationResponse { IsValid = result.IsValid, Message = result.Message };
        }

        var metadata = providerRegistry.GetMetadata(request.ProviderKind);
        connection = new AiProviderConnection
        {
            OrganizationId = organizationId,
            ProviderKind = request.ProviderKind,
            ApiKey = request.ApiKey?.Trim() ?? string.Empty,
            BaseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? metadata.DefaultBaseUrl : request.BaseUrl.Trim(),
            DeploymentName = request.DeploymentName,
            ApiVersion = request.ApiVersion,
        };

        var validation = await providerFactory.GetProvider(request.ProviderKind)
            .ValidateCredentialsAsync(connection, cancellationToken);
        return new AiProviderValidationResponse { IsValid = validation.IsValid, Message = validation.Message };
    }
}
