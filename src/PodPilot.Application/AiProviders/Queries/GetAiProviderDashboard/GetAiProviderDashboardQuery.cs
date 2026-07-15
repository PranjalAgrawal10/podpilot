using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.GetAiProviderDashboard;

/// <summary>Gets the AI provider dashboard summary.</summary>
public sealed class GetAiProviderDashboardQuery : IRequest<AiProviderDashboardResponse>
{
}

/// <summary>Handles <see cref="GetAiProviderDashboardQuery"/>.</summary>
public sealed class GetAiProviderDashboardQueryHandler : IRequestHandler<GetAiProviderDashboardQuery, AiProviderDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IAiProviderService aiProviderService;

    /// <summary>Initializes a new instance of the <see cref="GetAiProviderDashboardQueryHandler"/> class.</summary>
    public GetAiProviderDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IAiProviderService aiProviderService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.aiProviderService = aiProviderService;
    }

    /// <inheritdoc />
    public async Task<AiProviderDashboardResponse> Handle(
        GetAiProviderDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        var dashboard = await aiProviderService.GetDashboardAsync(organizationId, cancellationToken);
        return AiProviderMapper.ToDashboardResponse(dashboard);
    }
}
