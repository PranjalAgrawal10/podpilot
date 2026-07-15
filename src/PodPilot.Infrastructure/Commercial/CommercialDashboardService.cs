using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Aggregates commercial dashboard data for an organization.
/// </summary>
public sealed class CommercialDashboardService : ICommercialDashboardService
{
    private readonly ISubscriptionService subscriptionService;
    private readonly IUsageMeteringService usageMeteringService;
    private readonly ILicenseService licenseService;
    private readonly IReleaseService releaseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommercialDashboardService"/> class.
    /// </summary>
    public CommercialDashboardService(
        ISubscriptionService subscriptionService,
        IUsageMeteringService usageMeteringService,
        ILicenseService licenseService,
        IReleaseService releaseService)
    {
        this.subscriptionService = subscriptionService;
        this.usageMeteringService = usageMeteringService;
        this.licenseService = licenseService;
        this.releaseService = releaseService;
    }

    /// <inheritdoc />
    public async Task<CommercialDashboard> GetAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionService.GetOrCreateAsync(organizationId, cancellationToken);
        var usage = await usageMeteringService.GetUsageAsync(organizationId, cancellationToken);
        var license = await licenseService.ValidateAsync(organizationId, cancellationToken);
        var release = await releaseService.GetStatusAsync(cancellationToken);
        var remaining = Math.Max(0m, 100m - usage.RequestsQuotaPercent);

        return new CommercialDashboard
        {
            Subscription = subscription,
            Usage = usage,
            License = license,
            Release = release,
            EstimatedMonthlyCostUsd = usage.EstimatedMonthlyCostUsd,
            RemainingRequestQuotaPercent = Math.Round(remaining, 2),
        };
    }
}
