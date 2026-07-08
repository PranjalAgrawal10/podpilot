using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands.RestartPod;

/// <summary>
/// Handles restarting a GPU pod.
/// </summary>
public sealed class RestartPodCommandHandler : IRequestHandler<RestartPodCommand, PodResponse>
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
    /// Initializes a new instance of the <see cref="RestartPodCommandHandler"/> class.
    /// </summary>
    public RestartPodCommandHandler(
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
    public Task<PodResponse> Handle(RestartPodCommand request, CancellationToken cancellationToken) =>
        PodLifecycleHandler.ExecuteAsync(
            request.PodId,
            PodStatus.Restarting,
            "Pod restart requested.",
            "Pod restarted",
            (service, provider, providerPodId, token) => service.RestartPodAsync(provider, providerPodId, token),
            currentUserService,
            organizationAuthorizationService,
            dbContext,
            podService,
            podNotificationService,
            auditService,
            httpContextService,
            dateTimeService,
            cancellationToken);
}
