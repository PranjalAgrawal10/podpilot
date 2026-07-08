using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands.SyncPod;

/// <summary>
/// Handles synchronizing a pod's status with the provider.
/// </summary>
public sealed class SyncPodCommandHandler : IRequestHandler<SyncPodCommand, PodResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService podNotificationService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncPodCommandHandler"/> class.
    /// </summary>
    public SyncPodCommandHandler(
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
    public async Task<PodResponse> Handle(SyncPodCommand request, CancellationToken cancellationToken)
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

        if (pod.Status == PodStatus.Deleted)
        {
            throw new Common.Exceptions.NotFoundException("Pod", request.PodId);
        }

        await PodSyncHelper.SyncWithProviderAsync(
            pod,
            organizationId,
            podService,
            dbContext,
            podNotificationService,
            dateTimeService,
            "Status synchronized.",
            cancellationToken);

        return PodMapper.ToResponse(pod, includeHistory: true);
    }
}
