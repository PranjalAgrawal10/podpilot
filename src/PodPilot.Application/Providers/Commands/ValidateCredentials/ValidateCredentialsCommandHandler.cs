using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Providers.Commands.ValidateCredentials;

/// <summary>
/// Handles credential validation before provider creation.
/// </summary>
public sealed class ValidateCredentialsCommandHandler
    : IRequestHandler<ValidateCredentialsCommand, ProviderValidationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IProviderService providerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateCredentialsCommandHandler"/> class.
    /// </summary>
    public ValidateCredentialsCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IProviderService providerService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.providerService = providerService;
    }

    /// <inheritdoc />
    public async Task<ProviderValidationResponse> Handle(
        ValidateCredentialsCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderCreate,
            cancellationToken);

        var validation = await providerService.ValidateCredentialsAsync(
            request.ProviderType,
            request.ApiKey.Trim(),
            cancellationToken);

        return ProviderValidationMapper.ToResponse(validation);
    }
}
