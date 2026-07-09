using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetCost;

/// <summary>
/// Handles getting cost summary.
/// </summary>
public sealed class GetCostQueryHandler : IRequestHandler<GetCostQuery, CostResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly ICostCalculator costCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCostQueryHandler"/> class.
    /// </summary>
    public GetCostQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        ICostCalculator costCalculator)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.costCalculator = costCalculator;
    }

    /// <inheritdoc />
    public async Task<CostResponse> Handle(GetCostQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var summary = await costCalculator.CalculateAsync(
            organizationId,
            request.Period,
            request.ProviderId,
            request.PodId,
            request.ModelName,
            cancellationToken);

        return ObservabilityMapper.ToCostResponse(summary);
    }
}
