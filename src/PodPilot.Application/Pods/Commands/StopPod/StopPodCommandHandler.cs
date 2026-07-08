using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands.StopPod;

/// <summary>
/// Handles stopping a GPU pod.
/// </summary>
public sealed class StopPodCommandHandler : IRequestHandler<StopPodCommand, PodResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService podNotificationService;
    private readonly IPodLifecycleService podLifecycleService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StopPodCommandHandler"/> class.
    /// </summary>
    public StopPodCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodService podService,
        IPodNotificationService podNotificationService,
        IPodLifecycleService podLifecycleService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.podService = podService;
        this.podNotificationService = podNotificationService;
        this.podLifecycleService = podLifecycleService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public Task<PodResponse> Handle(StopPodCommand request, CancellationToken cancellationToken) =>
        PodLifecycleHandler.ExecuteAsync(
            request.PodId,
            PodStatus.Stopping,
            "Pod stop requested.",
            "Pod stopped",
            (service, provider, providerPodId, token) => service.StopPodAsync(provider, providerPodId, token),
            currentUserService,
            organizationAuthorizationService,
            dbContext,
            podService,
            podNotificationService,
            podLifecycleService,
            auditService,
            httpContextService,
            dateTimeService,
            cancellationToken);
}
