using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.ListAiProviderModels;

/// <summary>Lists AI provider models for the current organization.</summary>
public sealed class ListAiProviderModelsQuery : IRequest<IReadOnlyList<AiProviderModelResponse>>
{
    /// <summary>Gets or sets an optional provider filter.</summary>
    public Guid? ProviderId { get; init; }
}

/// <summary>Handles <see cref="ListAiProviderModelsQuery"/>.</summary>
public sealed class ListAiProviderModelsQueryHandler : IRequestHandler<ListAiProviderModelsQuery, IReadOnlyList<AiProviderModelResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>Initializes a new instance of the <see cref="ListAiProviderModelsQueryHandler"/> class.</summary>
    public ListAiProviderModelsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiProviderModelResponse>> Handle(
        ListAiProviderModelsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        var query = dbContext.AiProviderModels
            .AsNoTracking()
            .Include(m => m.AiProvider)
            .Where(m => m.OrganizationId == organizationId);

        if (request.ProviderId.HasValue)
        {
            query = query.Where(m => m.AiProviderId == request.ProviderId.Value);
        }

        var models = await query
            .OrderBy(m => m.ModelName)
            .ToListAsync(cancellationToken);

        return models.Select(AiProviderMapper.ToModelResponse).ToList();
    }
}
