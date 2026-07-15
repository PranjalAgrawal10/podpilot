using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.GetAiProvider;

/// <summary>Gets an AI provider by identifier.</summary>
public sealed class GetAiProviderQuery : IRequest<AiProviderResponse>
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; init; }
}

/// <summary>Handles <see cref="GetAiProviderQuery"/>.</summary>
public sealed class GetAiProviderQueryHandler : IRequestHandler<GetAiProviderQuery, AiProviderResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>Initializes a new instance of the <see cref="GetAiProviderQueryHandler"/> class.</summary>
    public GetAiProviderQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AiProviderResponse> Handle(GetAiProviderQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        var provider = await AiProviderAccess.GetProviderAsync(
            dbContext, request.ProviderId, organizationId, cancellationToken);
        return AiProviderMapper.ToResponse(provider);
    }
}
