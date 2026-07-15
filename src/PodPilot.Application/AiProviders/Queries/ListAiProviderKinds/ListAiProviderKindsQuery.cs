using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.ListAiProviderKinds;

/// <summary>Lists supported AI provider kinds.</summary>
public sealed class ListAiProviderKindsQuery : IRequest<IReadOnlyList<AiProviderKindMetadataResponse>>
{
}

/// <summary>Handles <see cref="ListAiProviderKindsQuery"/>.</summary>
public sealed class ListAiProviderKindsQueryHandler : IRequestHandler<ListAiProviderKindsQuery, IReadOnlyList<AiProviderKindMetadataResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IAiProviderRegistry providerRegistry;

    /// <summary>Initializes a new instance of the <see cref="ListAiProviderKindsQueryHandler"/> class.</summary>
    public ListAiProviderKindsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IAiProviderRegistry providerRegistry)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.providerRegistry = providerRegistry;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiProviderKindMetadataResponse>> Handle(
        ListAiProviderKindsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        return providerRegistry.ListMetadata()
            .Select(AiProviderMapper.ToKindMetadataResponse)
            .ToList();
    }
}
