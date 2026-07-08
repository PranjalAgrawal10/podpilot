using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Queries.ListPods;

/// <summary>
/// Handles listing organization pods.
/// </summary>
public sealed class ListPodsQueryHandler : IRequestHandler<ListPodsQuery, IReadOnlyList<PodResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService podNotificationService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPodsQueryHandler"/> class.
    /// </summary>
    public ListPodsQueryHandler(
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
    public async Task<IReadOnlyList<PodResponse>> Handle(
        ListPodsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodRead,
            cancellationToken);

        var providers = await dbContext.ComputeProviders
            .Include(p => p.Credential)
            .Where(p => p.OrganizationId == organizationId && p.IsEnabled && p.IsValidated)
            .ToListAsync(cancellationToken);

        foreach (var provider in providers)
        {
            try
            {
                await podService.ImportProviderPodsAsync(provider, organizationId, cancellationToken);
            }
            catch
            {
                // Listing should still return locally tracked pods when provider import fails.
            }
        }

        var pods = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId
                && p.Status != PodStatus.Deleted
                && p.Status != PodStatus.Deleting)
            .Include(p => p.Provider)
                .ThenInclude(provider => provider.Credential)
            .Include(p => p.Endpoints)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .ToListAsync(cancellationToken);

        var utcNow = dateTimeService.UtcNow;
        foreach (var pod in pods.Where(p => PodSyncHelper.IsStale(p, utcNow)))
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
                // Keep listing responsive when a single provider sync fails.
            }
        }

        return pods
            .Where(p => PodSyncHelper.IsVisible(p.Status))
            .Select(p => PodMapper.ToResponse(p))
            .ToList();
    }
}
