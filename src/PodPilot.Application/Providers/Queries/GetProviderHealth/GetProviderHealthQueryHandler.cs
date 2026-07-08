using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Providers.Queries.GetProviderHealth;

/// <summary>
/// Handles getting provider health.
/// </summary>
public sealed class GetProviderHealthQueryHandler : IRequestHandler<GetProviderHealthQuery, ProviderHealthResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProviderHealthQueryHandler"/> class.
    /// </summary>
    public GetProviderHealthQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResponse> Handle(
        GetProviderHealthQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderRead,
            cancellationToken);

        await ProviderAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken);

        var health = await dbContext.ProviderHealthSnapshots
            .FirstOrDefaultAsync(h => h.ComputeProviderId == request.ProviderId, cancellationToken);

        return new ProviderHealthResponse
        {
            ProviderId = request.ProviderId,
            Status = (health?.Status ?? Domain.Enums.ProviderConnectionStatus.Unknown).ToString(),
            LastCheckedAt = health?.LastCheckedAt,
            ResponseTimeMs = health?.ResponseTimeMs,
            ErrorMessage = health?.ErrorMessage,
        };
    }
}
