using MediatR;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands;

/// <summary>
/// Shared lifecycle operation handler for GPU pods.
/// </summary>
internal static class PodLifecycleHandler
{
    /// <summary>
    /// Executes a pod lifecycle operation.
    /// </summary>
    public static async Task<PodResponse> ExecuteAsync(
        Guid podId,
        PodStatus transitionalStatus,
        string historyMessage,
        string auditMessage,
        Func<IPodService, Domain.Entities.ComputeProvider, string, CancellationToken, Task<Models.Pods.PodOperationResult>> operation,
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodService podService,
        IPodNotificationService podNotificationService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodUpdate,
            cancellationToken);

        var pod = await PodAccess.GetPodAsync(
            dbContext,
            podId,
            organizationId,
            cancellationToken,
            includeDetails: true);

        if (pod.Status == PodStatus.Deleted)
        {
            throw new NotFoundException("Pod", podId);
        }

        if (string.IsNullOrWhiteSpace(pod.ProviderPodId))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(podId),
                    "Pod has not been provisioned on the provider yet."),
            ]);
        }

        var now = dateTimeService.UtcNow;
        pod.Status = transitionalStatus;
        pod.UpdatedAt = now;
        pod.UpdatedBy = userId.ToString();

        await dbContext.AddPodStatusHistoryAsync(
            new PodStatusHistory
            {
                GpuPodId = pod.Id,
                Status = transitionalStatus,
                RecordedAt = now,
                Message = historyMessage,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await podNotificationService.NotifyPodStatusChangedAsync(
            organizationId,
            pod.Id,
            pod.Status.ToString(),
            cancellationToken);

        var result = await operation(podService, pod.Provider, pod.ProviderPodId, cancellationToken);
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
            await podNotificationService.NotifyPodStatusChangedAsync(
                organizationId,
                pod.Id,
                pod.Status.ToString(),
                cancellationToken);
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(podId),
                    result.ErrorMessage ?? "Pod operation failed."),
            ]);
        }

        if (result.Pod is not null)
        {
            podService.ApplyProviderStatus(pod, result.Pod, dateTimeService.UtcNow);
        }
        else
        {
            pod.Status = result.Status;
        }

        pod.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.AddPodStatusHistoryAsync(
            new PodStatusHistory
            {
                GpuPodId = pod.Id,
                Status = pod.Status,
                RecordedAt = dateTimeService.UtcNow,
                Message = result.Pod?.StatusMessage,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await podNotificationService.NotifyPodStatusChangedAsync(
            organizationId,
            pod.Id,
            pod.Status.ToString(),
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(GpuPod),
            pod.Id.ToString(),
            auditMessage,
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return PodMapper.ToResponse(pod, includeHistory: true);
    }
}
