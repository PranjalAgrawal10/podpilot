using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.ListAiProviders;

/// <summary>Lists AI providers for the current organization.</summary>
public sealed class ListAiProvidersQuery : IRequest<IReadOnlyList<AiProviderResponse>>
{
}

/// <summary>Handles <see cref="ListAiProvidersQuery"/>.</summary>
public sealed class ListAiProvidersQueryHandler : IRequestHandler<ListAiProvidersQuery, IReadOnlyList<AiProviderResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>Initializes a new instance of the <see cref="ListAiProvidersQueryHandler"/> class.</summary>
    public ListAiProvidersQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiProviderResponse>> Handle(ListAiProvidersQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        var providers = await dbContext.AiInferenceProviders
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return providers.Select(AiProviderMapper.ToResponse).ToList();
    }
}
