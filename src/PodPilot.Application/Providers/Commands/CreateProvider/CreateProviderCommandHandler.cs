using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Providers.Commands.CreateProvider;

/// <summary>
/// Handles compute provider creation.
/// </summary>
public sealed class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, ProviderResponse>
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
    /// Initializes a new instance of the <see cref="CreateProviderCommandHandler"/> class.
    /// </summary>
    public CreateProviderCommandHandler(
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
        CreateProviderCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderCreate,
            cancellationToken);

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.ComputeProviders.AnyAsync(
            p => p.OrganizationId == organizationId && p.Name == normalizedName,
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

        var validation = await providerService.ValidateCredentialsAsync(
            request.ProviderType,
            request.ApiKey.Trim(),
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

        var now = dateTimeService.UtcNow;
        var provider = new ComputeProvider
        {
            OrganizationId = organizationId,
            Name = normalizedName,
            ProviderType = request.ProviderType,
            DisplayName = request.DisplayName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DefaultRegion = string.IsNullOrWhiteSpace(request.DefaultRegion) ? null : request.DefaultRegion.Trim(),
            IsEnabled = request.IsEnabled,
            IsValidated = true,
            LastValidatedAt = now,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        var credential = new ProviderCredential
        {
            ComputeProviderId = provider.Id,
            EncryptedApiKey = encryptionService.Encrypt(request.ApiKey.Trim()),
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        provider.Credential = credential;

        provider.Health = new ProviderHealth
        {
            ComputeProviderId = provider.Id,
            Status = ProviderConnectionStatus.Connected,
            LastCheckedAt = now,
        };

        await dbContext.AddComputeProviderAsync(provider, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await providerService.SyncCatalogAsync(provider, request.ApiKey.Trim(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(ComputeProvider),
            provider.Id.ToString(),
            $"Provider '{provider.DisplayName}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return ProviderMapper.ToResponse(provider);
    }
}
