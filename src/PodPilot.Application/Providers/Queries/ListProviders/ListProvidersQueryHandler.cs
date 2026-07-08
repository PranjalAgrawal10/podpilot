using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Providers.Queries.ListProviders;

/// <summary>
/// Handles listing compute providers.
/// </summary>
public sealed class ListProvidersQueryHandler : IRequestHandler<ListProvidersQuery, IReadOnlyList<ProviderResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListProvidersQueryHandler"/> class.
    /// </summary>
    public ListProvidersQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderResponse>> Handle(
        ListProvidersQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ProviderAccess.RequireOrganizationContext(currentUserService);

        await ProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ProviderRead,
            cancellationToken);

        var providers = await dbContext.ComputeProviders
            .Where(p => p.OrganizationId == organizationId)
            .OrderBy(p => p.DisplayName)
            .ToListAsync(cancellationToken);

        return providers.Select(ProviderMapper.ToResponse).ToList();
    }
}
