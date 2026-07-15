using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.GetAiProviderHealth;

/// <summary>Gets AI provider health, optionally refreshing it.</summary>
public sealed class GetAiProviderHealthQuery : IRequest<AiProviderHealthResponse>
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets a value indicating whether to refresh health.</summary>
    public bool Refresh { get; init; } = true;
}

/// <summary>Handles <see cref="GetAiProviderHealthQuery"/>.</summary>
public sealed class GetAiProviderHealthQueryHandler : IRequestHandler<GetAiProviderHealthQuery, AiProviderHealthResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderService aiProviderService;

    /// <summary>Initializes a new instance of the <see cref="GetAiProviderHealthQueryHandler"/> class.</summary>
    public GetAiProviderHealthQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAiProviderService aiProviderService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.aiProviderService = aiProviderService;
    }

    /// <inheritdoc />
    public async Task<AiProviderHealthResponse> Handle(GetAiProviderHealthQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        if (request.Refresh)
        {
            return await aiProviderService.CheckHealthAsync(organizationId, request.ProviderId, cancellationToken);
        }

        var provider = await AiProviderAccess.GetProviderAsync(
            dbContext, request.ProviderId, organizationId, cancellationToken, includeHealth: true);

        if (provider.Health is null)
        {
            return await aiProviderService.CheckHealthAsync(organizationId, request.ProviderId, cancellationToken);
        }

        return AiProviderMapper.ToHealthResponse(provider.Health);
    }
}
