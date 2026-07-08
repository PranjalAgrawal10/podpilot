using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Queries.GetPod;

/// <summary>
/// Handles retrieving a pod by identifier.
/// </summary>
public sealed class GetPodQueryHandler : IRequestHandler<GetPodQuery, PodResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService podNotificationService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPodQueryHandler"/> class.
    /// </summary>
    public GetPodQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodService podService,
        IPodNotificationService podNotificationService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.podService = podService;
        this.podNotificationService = podNotificationService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<PodResponse> Handle(GetPodQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodRead,
            cancellationToken);

        var pod = await PodAccess.GetPodAsync(
            dbContext,
            request.PodId,
            organizationId,
            cancellationToken,
            includeDetails: true);

        if (pod.Status == PodStatus.Deleted || pod.Status == PodStatus.Deleting)
        {
            throw new Common.Exceptions.NotFoundException("Pod", request.PodId);
        }

        if (PodSyncHelper.IsStale(pod, dateTimeService.UtcNow))
        {
            try
            {
                await PodSyncHelper.SyncWithProviderAsync(
                    pod,
                    organizationId,
                    podService,
                    dbContext,
                    podNotificationService,
                    dateTimeService,
                    "Status synchronized.",
                    cancellationToken);
            }
            catch
            {
                // Return the last known pod state when provider sync fails.
            }
        }

        if (pod.Status == PodStatus.Deleted || pod.Status == PodStatus.Deleting)
        {
            throw new Common.Exceptions.NotFoundException("Pod", request.PodId);
        }

        return PodMapper.ToResponse(pod, includeHistory: true);
    }
}
