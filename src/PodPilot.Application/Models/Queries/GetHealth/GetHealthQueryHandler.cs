using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Queries.GetHealth;

/// <summary>
/// Handles model health queries.
/// </summary>
public sealed class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, IReadOnlyList<ModelHealthResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetHealthQueryHandler"/> class.
    /// </summary>
    public GetHealthQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelHealthResponse>> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelRead,
            cancellationToken);

        var query = dbContext.ModelHealthHistory
            .Include(h => h.Model)
            .Where(h => h.Model.OrganizationId == organizationId);

        if (request.ModelId.HasValue)
        {
            query = query.Where(h => h.ModelId == request.ModelId.Value);
        }

        var records = await query
            .OrderByDescending(h => h.LastChecked)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return records.Select(ModelMapper.ToHealthResponse).ToList();
    }
}
