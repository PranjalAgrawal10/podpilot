using MediatR;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Gateway;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Commands.CreateGatewayApiKey;

/// <summary>
/// Handles gateway API key creation.
/// </summary>
public sealed class CreateGatewayApiKeyCommandHandler : IRequestHandler<CreateGatewayApiKeyCommand, GatewayApiKeyResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IGatewayApiKeyService gatewayApiKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGatewayApiKeyCommandHandler"/> class.
    /// </summary>
    public CreateGatewayApiKeyCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IGatewayApiKeyService gatewayApiKeyService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.gatewayApiKeyService = gatewayApiKeyService;
    }

    /// <inheritdoc />
    public async Task<GatewayApiKeyResponse> Handle(CreateGatewayApiKeyCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayManage,
            cancellationToken);

        var (entity, plaintextKey) = await gatewayApiKeyService.CreateKeyAsync(
            organizationId,
            request.IsPersonal ? userId : null,
            request.Name,
            request.ExpiresAt,
            request.RateLimitPerMinute ?? ApplicationConstants.DefaultGatewayRateLimitPerMinute,
            request.RateLimitPerDay ?? ApplicationConstants.DefaultGatewayRateLimitPerDay,
            cancellationToken);

        return GatewayMapper.ToApiKeyResponse(entity, plaintextKey);
    }
}
