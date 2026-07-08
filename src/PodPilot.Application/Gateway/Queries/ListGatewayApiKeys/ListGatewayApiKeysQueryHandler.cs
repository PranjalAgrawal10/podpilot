using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Gateway;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Gateway.Queries.ListGatewayApiKeys;

/// <summary>
/// Handles listing gateway API keys.
/// </summary>
public sealed class ListGatewayApiKeysQueryHandler : IRequestHandler<ListGatewayApiKeysQuery, IReadOnlyList<GatewayApiKeyResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListGatewayApiKeysQueryHandler"/> class.
    /// </summary>
    public ListGatewayApiKeysQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GatewayApiKeyResponse>> Handle(
        ListGatewayApiKeysQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var keys = await dbContext.GatewayApiKeys
            .Where(k => k.OrganizationId == organizationId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);

        return keys.Select(k => GatewayMapper.ToApiKeyResponse(k)).ToList();
    }
}
