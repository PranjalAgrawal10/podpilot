using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Looks up the active organization routing policy.
/// </summary>
public sealed class RoutingPolicyService : IRoutingPolicy
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingPolicyService"/> class.
    /// </summary>
    public RoutingPolicyService(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AiRoutingPolicy?> GetActivePolicyAsync(
        Guid organizationId,
        string? modelHint,
        CancellationToken cancellationToken = default)
    {
        var policies = await dbContext.AiRoutingPolicies
            .AsNoTracking()
            .Include(p => p.PrimaryProvider)
            .Where(p => p.OrganizationId == organizationId && p.IsEnabled)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(modelHint) &&
            !string.Equals(modelHint, "auto", StringComparison.OrdinalIgnoreCase))
        {
            var modelMatch = policies.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.ModelName) &&
                string.Equals(p.ModelName, modelHint, StringComparison.OrdinalIgnoreCase));
            if (modelMatch is not null)
            {
                return modelMatch;
            }
        }

        return policies.FirstOrDefault(p => p.IsDefault)
               ?? policies.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.ModelName))
               ?? policies.FirstOrDefault();
    }
}
