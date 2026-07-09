using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetSystemHealth;

/// <summary>
/// Handles getting system health.
/// </summary>
public sealed class GetSystemHealthQueryHandler : IRequestHandler<GetSystemHealthQuery, SystemHealthResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IMonitoringService monitoringService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSystemHealthQueryHandler"/> class.
    /// </summary>
    public GetSystemHealthQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IMonitoringService monitoringService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.monitoringService = monitoringService;
    }

    /// <inheritdoc />
    public async Task<SystemHealthResponse> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var overview = await monitoringService.RunHealthChecksAsync(organizationId, cancellationToken);
        return ObservabilityMapper.ToSystemHealthResponse(overview);
    }
}
