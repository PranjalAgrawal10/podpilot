using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Providers.Queries.ListProviderRegions;

/// <summary>
/// Handles listing provider regions.
/// </summary>
public sealed class ListProviderRegionsQueryHandler
    : IRequestHandler<ListProviderRegionsQuery, IReadOnlyList<ProviderRegionResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IProviderService providerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListProviderRegionsQueryHandler"/> class.
    /// </summary>
    public ListProviderRegionsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IProviderService providerService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.providerService = providerService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderRegionResponse>> Handle(
        ListProviderRegionsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderRead,
            cancellationToken);

        var provider = await ProviderAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken,
            includeCredential: true);

        var regions = await providerService.ListRegionsAsync(provider, cancellationToken);

        return regions
            .Select(r => new ProviderRegionResponse
            {
                RegionId = r.RegionId,
                Name = r.Name,
                IsAvailable = r.IsAvailable,
            })
            .ToList();
    }
}
