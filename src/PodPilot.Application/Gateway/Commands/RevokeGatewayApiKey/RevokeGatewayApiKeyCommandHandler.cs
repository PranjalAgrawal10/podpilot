using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Commands.RevokeGatewayApiKey;

/// <summary>
/// Handles gateway API key revocation.
/// </summary>
public sealed class RevokeGatewayApiKeyCommandHandler : IRequestHandler<RevokeGatewayApiKeyCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IGatewayApiKeyService gatewayApiKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevokeGatewayApiKeyCommandHandler"/> class.
    /// </summary>
    public RevokeGatewayApiKeyCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IGatewayApiKeyService gatewayApiKeyService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.gatewayApiKeyService = gatewayApiKeyService;
    }

    /// <inheritdoc />
    public async Task Handle(RevokeGatewayApiKeyCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayManage,
            cancellationToken);

        await gatewayApiKeyService.RevokeKeyAsync(request.KeyId, organizationId, cancellationToken);
    }
}
