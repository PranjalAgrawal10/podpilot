using MediatR;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands.DeletePod;

/// <summary>
/// Handles GPU pod deletion.
/// </summary>
public sealed class DeletePodCommandHandler : IRequestHandler<DeletePodCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService podNotificationService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePodCommandHandler"/> class.
    /// </summary>
    public DeletePodCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodService podService,
        IPodNotificationService podNotificationService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.podService = podService;
        this.podNotificationService = podNotificationService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task Handle(DeletePodCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodDelete,
            cancellationToken);

        var pod = await PodAccess.GetPodAsync(
            dbContext,
            request.PodId,
            organizationId,
            cancellationToken);

        if (pod.Status == PodStatus.Deleted)
        {
            return;
        }

        if (!request.Force && pod.Status is PodStatus.Running or PodStatus.Starting or PodStatus.Restarting or PodStatus.BuildingPending)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Force),
                    "Cannot delete a running pod without confirmation. Set force=true to proceed."),
            ]);
        }

        var now = dateTimeService.UtcNow;
        pod.Status = PodStatus.Deleting;
        pod.UpdatedAt = now;
        pod.UpdatedBy = userId.ToString();

        await dbContext.AddPodStatusHistoryAsync(
            new PodStatusHistory
            {
                GpuPodId = pod.Id,
                Status = PodStatus.Deleting,
                RecordedAt = now,
                Message = "Pod deletion requested.",
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await podNotificationService.NotifyPodStatusChangedAsync(
            organizationId,
            pod.Id,
            pod.Status.ToString(),
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(pod.ProviderPodId))
        {
            var result = await podService.DeletePodAsync(pod.Provider, pod.ProviderPodId, cancellationToken);
            if (!result.Success)
            {
                pod.Status = PodStatus.Failed;
                await dbContext.AddPodStatusHistoryAsync(
                    new PodStatusHistory
                    {
                        GpuPodId = pod.Id,
                        Status = PodStatus.Failed,
                        RecordedAt = dateTimeService.UtcNow,
                        Message = result.ErrorMessage,
                    },
                    cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.PodId),
                        result.ErrorMessage ?? "Failed to delete pod on provider."),
                ]);
            }
        }

        pod.Status = PodStatus.Deleted;
        pod.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.AddPodStatusHistoryAsync(
            new PodStatusHistory
            {
                GpuPodId = pod.Id,
                Status = PodStatus.Deleted,
                RecordedAt = dateTimeService.UtcNow,
                Message = "Pod deleted.",
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await podNotificationService.NotifyPodStatusChangedAsync(
            organizationId,
            pod.Id,
            pod.Status.ToString(),
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(GpuPod),
            pod.Id.ToString(),
            $"Pod '{pod.Name}' deleted",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
    }
}
