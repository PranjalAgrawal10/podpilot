using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetAnalytics;

/// <summary>
/// Handles getting usage analytics.
/// </summary>
public sealed class GetAnalyticsQueryHandler : IRequestHandler<GetAnalyticsQuery, AnalyticsResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IAnalyticsService analyticsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalyticsQueryHandler"/> class.
    /// </summary>
    public GetAnalyticsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IAnalyticsService analyticsService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.analyticsService = analyticsService;
    }

    /// <inheritdoc />
    public async Task<AnalyticsResponse> Handle(GetAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var summary = await analyticsService.GetAnalyticsAsync(
            organizationId,
            request.Period,
            request.From,
            request.To,
            request.ProviderId,
            request.PodId,
            request.ModelName,
            cancellationToken);

        return ObservabilityMapper.ToAnalyticsResponse(summary);
    }
}
