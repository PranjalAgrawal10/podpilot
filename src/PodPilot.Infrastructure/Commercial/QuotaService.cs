using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Enforces plan quotas for an organization.
/// </summary>
public sealed class QuotaService : IQuotaService
{
    private readonly IApplicationDbContext db;
    private readonly ISubscriptionService subscriptionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotaService"/> class.
    /// </summary>
    public QuotaService(IApplicationDbContext db, ISubscriptionService subscriptionService)
    {
        this.db = db;
        this.subscriptionService = subscriptionService;
    }

    /// <inheritdoc />
    public async Task<QuotaLimits> GetLimitsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionService.GetOrCreateAsync(organizationId, cancellationToken);
        return subscription.Quotas;
    }

    /// <inheritdoc />
    public async Task EnsureCanCreatePodAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var limits = await GetLimitsAsync(organizationId, cancellationToken);
        var count = await db.GpuPods.CountAsync(
            p => p.OrganizationId == organizationId && p.Status != PodStatus.Deleted,
            cancellationToken);

        if (count >= limits.MaxPods)
        {
            throw new ForbiddenException(
                $"Plan quota exceeded: maximum {limits.MaxPods} pods allowed on your current plan.");
        }
    }

    /// <inheritdoc />
    public async Task EnsureCanCreateProviderAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var limits = await GetLimitsAsync(organizationId, cancellationToken);
        var count = await db.ComputeProviders.CountAsync(p => p.OrganizationId == organizationId, cancellationToken);

        if (count >= limits.MaxProviders)
        {
            throw new ForbiddenException(
                $"Plan quota exceeded: maximum {limits.MaxProviders} providers allowed on your current plan.");
        }
    }

    /// <inheritdoc />
    public async Task EnsureApiRequestAllowedAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var limits = await GetLimitsAsync(organizationId, cancellationToken);
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var count = await db.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId && r.CreatedAt >= periodStart,
            cancellationToken);

        if (count >= limits.MaxApiRequestsPerMonth)
        {
            throw new ForbiddenException(
                $"Plan quota exceeded: maximum {limits.MaxApiRequestsPerMonth} API requests per month allowed on your current plan.");
        }
    }
}
