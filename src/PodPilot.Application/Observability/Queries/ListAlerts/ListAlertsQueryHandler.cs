using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.ListAlerts;

/// <summary>
/// Handles listing alerts.
/// </summary>
public sealed class ListAlertsQueryHandler : IRequestHandler<ListAlertsQuery, IReadOnlyList<AlertResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListAlertsQueryHandler"/> class.
    /// </summary>
    public ListAlertsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AlertResponse>> Handle(ListAlertsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var query = dbContext.AlertHistory
            .Where(a => a.OrganizationId == organizationId);

        if (request.ActiveOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        var alerts = await query
            .OrderByDescending(a => a.RaisedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return alerts.Select(ObservabilityMapper.ToAlertResponse).ToList();
    }
}
