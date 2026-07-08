using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands.UpdatePod;

/// <summary>
/// Handles GPU pod updates.
/// </summary>
public sealed class UpdatePodCommandHandler : IRequestHandler<UpdatePodCommand, PodResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePodCommandHandler"/> class.
    /// </summary>
    public UpdatePodCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<PodResponse> Handle(UpdatePodCommand request, CancellationToken cancellationToken)
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
            request.PodId,
            organizationId,
            cancellationToken,
            includeDetails: true);

        if (pod.Status == PodStatus.Deleted)
        {
            throw new NotFoundException("Pod", request.PodId);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim();
            var nameExists = await dbContext.GpuPods.AnyAsync(
                p => p.OrganizationId == organizationId
                    && p.Name == normalizedName
                    && p.Id != pod.Id
                    && p.Status != PodStatus.Deleted,
                cancellationToken);

            if (nameExists)
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.Name),
                        "A pod with this name already exists."),
                ]);
            }

            pod.Name = normalizedName;
        }

        if (request.Description is not null)
        {
            pod.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        }

        pod.UpdatedAt = dateTimeService.UtcNow;
        pod.UpdatedBy = userId.ToString();
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(GpuPod),
            pod.Id.ToString(),
            $"Pod '{pod.Name}' updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return PodMapper.ToResponse(pod, includeHistory: true);
    }
}
