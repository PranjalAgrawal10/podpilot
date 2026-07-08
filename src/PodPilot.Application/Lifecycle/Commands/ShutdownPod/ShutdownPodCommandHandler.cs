using MediatR;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Lifecycle.Commands.ShutdownPod;

/// <summary>
/// Handles shutdown pod commands.
/// </summary>
public sealed class ShutdownPodCommandHandler : IRequestHandler<ShutdownPodCommand, PodShutdownResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodLifecycleService lifecycleService;
    private readonly ILogger<ShutdownPodCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShutdownPodCommandHandler"/> class.
    /// </summary>
    public ShutdownPodCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodLifecycleService lifecycleService,
        ILogger<ShutdownPodCommandHandler> logger)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.lifecycleService = lifecycleService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<PodShutdownResponse> Handle(ShutdownPodCommand request, CancellationToken cancellationToken)
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

        logger.LogInformation("Shutdown requested for pod {PodId} by user {UserId}", request.PodId, userId);

        var result = await lifecycleService.ShutdownPodAsync(
            request.PodId,
            organizationId,
            "api",
            request.Reason,
            userId,
            cancellationToken);

        return new PodShutdownResponse
        {
            Success = result.Success,
            Status = result.Status,
            ErrorMessage = result.ErrorMessage,
        };
    }
}
