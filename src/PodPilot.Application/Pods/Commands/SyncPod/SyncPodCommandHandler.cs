using MediatR;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
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
            throw new NotFoundException("Pod", request.PodId);
        }

        if (string.IsNullOrWhiteSpace(pod.ProviderPodId))
        {
            return PodMapper.ToResponse(pod, includeHistory: true);
        }

        var previousStatus = pod.Status;
        var info = await podService.SyncPodStatusAsync(pod, cancellationToken);

        if (previousStatus != pod.Status)
        {
            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = dateTimeService.UtcNow,
                    Message = info.StatusMessage ?? "Status synchronized.",
                },
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await podNotificationService.NotifyPodStatusChangedAsync(
                organizationId,
                pod.Id,
                pod.Status.ToString(),
                cancellationToken);
        }

        return PodMapper.ToResponse(pod, includeHistory: true);
    }
}
