using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Gateway;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Commands.RotateGatewayApiKey;

/// <summary>
/// Handles gateway API key rotation.
/// </summary>
public sealed class RotateGatewayApiKeyCommandHandler : IRequestHandler<RotateGatewayApiKeyCommand, GatewayApiKeyResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IGatewayApiKeyService gatewayApiKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RotateGatewayApiKeyCommandHandler"/> class.
    /// </summary>
    public RotateGatewayApiKeyCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IGatewayApiKeyService gatewayApiKeyService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.gatewayApiKeyService = gatewayApiKeyService;
    }

    /// <inheritdoc />
    public async Task<GatewayApiKeyResponse> Handle(RotateGatewayApiKeyCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayManage,
            cancellationToken);

        var (entity, plaintextKey) = await gatewayApiKeyService.RotateKeyAsync(
            request.KeyId,
            organizationId,
            cancellationToken);

        return GatewayMapper.ToApiKeyResponse(entity, plaintextKey);
    }
}
