using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Providers.Queries.ListProviderGpus;

/// <summary>
/// Handles listing provider GPUs.
/// </summary>
public sealed class ListProviderGpusQueryHandler
    : IRequestHandler<ListProviderGpusQuery, IReadOnlyList<ProviderGpuResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IProviderService providerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListProviderGpusQueryHandler"/> class.
    /// </summary>
    public ListProviderGpusQueryHandler(
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
    public async Task<IReadOnlyList<ProviderGpuResponse>> Handle(
        ListProviderGpusQuery request,
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

        var gpus = await providerService.ListGpusAsync(provider, cancellationToken);

        return gpus
            .Select(g => new ProviderGpuResponse
            {
                GpuId = g.GpuId,
                Name = g.Name,
                GpuType = g.GpuType.ToString(),
                MemoryGb = g.MemoryGb,
                IsAvailable = g.IsAvailable,
            })
            .ToList();
    }
}
