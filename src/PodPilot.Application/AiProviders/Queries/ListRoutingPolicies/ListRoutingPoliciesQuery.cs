using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.AiProviders.Queries.ListRoutingPolicies;

/// <summary>Lists AI routing policies for the current organization.</summary>
public sealed class ListRoutingPoliciesQuery : IRequest<IReadOnlyList<AiRoutingPolicyResponse>>
{
}

/// <summary>Handles <see cref="ListRoutingPoliciesQuery"/>.</summary>
public sealed class ListRoutingPoliciesQueryHandler : IRequestHandler<ListRoutingPoliciesQuery, IReadOnlyList<AiRoutingPolicyResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>Initializes a new instance of the <see cref="ListRoutingPoliciesQueryHandler"/> class.</summary>
    public ListRoutingPoliciesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiRoutingPolicyResponse>> Handle(
        ListRoutingPoliciesQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.AiProviderRead, cancellationToken);

        var policies = await dbContext.AiRoutingPolicies
            .AsNoTracking()
            .Include(p => p.PrimaryProvider)
            .Where(p => p.OrganizationId == organizationId)
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return policies.Select(AiProviderMapper.ToRoutingPolicyResponse).ToList();
    }
}
