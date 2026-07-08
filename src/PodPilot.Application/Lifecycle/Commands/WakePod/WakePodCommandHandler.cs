using MediatR;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Lifecycle.Commands.WakePod;

/// <summary>
/// Handles wake pod commands.
/// </summary>
public sealed class WakePodCommandHandler : IRequestHandler<WakePodCommand, PodWakeResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodLifecycleService lifecycleService;
    private readonly ILogger<WakePodCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WakePodCommandHandler"/> class.
    /// </summary>
    public WakePodCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodLifecycleService lifecycleService,
        ILogger<WakePodCommandHandler> logger)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.lifecycleService = lifecycleService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<PodWakeResponse> Handle(WakePodCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodUpdate,
            cancellationToken);

        var pod = await PodAccess.GetPodAsync(dbContext, request.PodId, organizationId, cancellationToken);
        if (pod.Status == PodStatus.Deleted)
        {
            throw new Common.Exceptions.NotFoundException("Pod", request.PodId);
        }

        logger.LogInformation("Wake requested for pod {PodId} by user {UserId}", request.PodId, userId);

        var result = await lifecycleService.WakePodAsync(
            request.PodId,
            organizationId,
            "api",
            userId,
            processImmediately: false,
            cancellationToken);

        return new PodWakeResponse
        {
            Success = result.Success,
            Queued = result.Queued,
            WakeRequestId = result.WakeRequestId,
            Status = result.Status,
            ErrorMessage = result.ErrorMessage,
        };
    }
}
