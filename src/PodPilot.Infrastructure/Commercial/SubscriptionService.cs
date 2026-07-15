using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Manages organization subscriptions and plan catalog.
/// </summary>
public sealed class SubscriptionService : ISubscriptionService
{
    private readonly IApplicationDbContext db;
    private readonly PlanCatalogSeeder catalogSeeder;
    private readonly IPaymentGatewayFactory paymentGatewayFactory;
    private readonly ICommercialNotificationService notifications;
    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionService"/> class.
    /// </summary>
    public SubscriptionService(
        IApplicationDbContext db,
        PlanCatalogSeeder catalogSeeder,
        IPaymentGatewayFactory paymentGatewayFactory,
        ICommercialNotificationService notifications,
        ICurrentUserService currentUserService)
    {
        this.db = db;
        this.catalogSeeder = catalogSeeder;
        this.paymentGatewayFactory = paymentGatewayFactory;
        this.notifications = notifications;
        this.currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public Task EnsureCatalogAsync(CancellationToken cancellationToken = default) =>
        catalogSeeder.EnsureAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<OrganizationSubscriptionInfo> GetOrCreateAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCatalogAsync(cancellationToken);

        var subscription = await db.OrganizationSubscriptions
            .Include(s => s.SubscriptionPlan)
            .ThenInclude(p => p.Quota)
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, cancellationToken);

        if (subscription is null)
        {
            var freePlan = await db.SubscriptionPlans
                .Include(p => p.Quota)
                .FirstAsync(p => p.Code == "free", cancellationToken);

            var now = DateTime.UtcNow;
            subscription = new OrganizationSubscription
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                SubscriptionPlanId = freePlan.Id,
                Status = SubscriptionStatus.Active,
                BillingInterval = BillingInterval.Monthly,
                SeatCount = freePlan.IncludedSeats,
                CurrentPeriodStart = now,
                CurrentPeriodEnd = now.AddMonths(1),
                CreatedAt = now,
            };

            await db.AddOrganizationSubscriptionAsync(subscription, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            subscription.SubscriptionPlan = freePlan;
        }

        return Map(subscription);
    }

    /// <inheritdoc />
    public async Task<CheckoutSessionResult> StartCheckoutAsync(
        Guid organizationId,
        StartCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCatalogAsync(cancellationToken);
        _ = await GetOrCreateAsync(organizationId, cancellationToken);

        var plan = await db.SubscriptionPlans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == request.PlanCode && p.IsPublic, cancellationToken)
            ?? throw new NotFoundException("Subscription plan", request.PlanCode);

        if (plan.Tier == SubscriptionPlanTier.Free)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.PlanCode),
                    "Cannot checkout the Free plan."),
            ]);
        }

        var gateway = paymentGatewayFactory.Get(request.Provider);
        var email = currentUserService.Email ?? string.Empty;
        return await gateway.CreateCheckoutAsync(
            new CheckoutSessionRequest
            {
                OrganizationId = organizationId,
                PlanCode = plan.Code,
                Interval = request.Interval,
                SeatCount = Math.Max(1, request.SeatCount),
                SuccessUrl = string.IsNullOrWhiteSpace(request.SuccessUrl)
                    ? "https://app.podpilot.local/billing/success"
                    : request.SuccessUrl,
                CancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl)
                    ? "https://app.podpilot.local/billing/cancel"
                    : request.CancelUrl,
                CustomerEmail = email,
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task CancelAsync(Guid organizationId, bool atPeriodEnd, CancellationToken cancellationToken = default)
    {
        var subscription = await db.OrganizationSubscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization subscription", organizationId);

        if (!string.IsNullOrWhiteSpace(subscription.ExternalSubscriptionId) &&
            subscription.PaymentProvider.HasValue)
        {
            var gateway = paymentGatewayFactory.Get(subscription.PaymentProvider.Value);
            await gateway.CancelSubscriptionAsync(subscription.ExternalSubscriptionId, atPeriodEnd, cancellationToken);
        }

        if (atPeriodEnd)
        {
            subscription.CancelAtPeriodEnd = true;
        }
        else
        {
            var freePlan = await db.SubscriptionPlans.FirstAsync(p => p.Code == "free", cancellationToken);
            subscription.SubscriptionPlanId = freePlan.Id;
            subscription.Status = SubscriptionStatus.Canceled;
            subscription.CancelAtPeriodEnd = false;
            subscription.PaymentProvider = null;
            subscription.ExternalSubscriptionId = null;
        }

        subscription.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await notifications.NotifySubscriptionChangedAsync(
            organizationId,
            subscription.Status.ToString(),
            cancellationToken);
    }

    internal static OrganizationSubscriptionInfo Map(OrganizationSubscription subscription)
    {
        var plan = subscription.SubscriptionPlan;
        var quota = plan?.Quota;
        return new OrganizationSubscriptionInfo
        {
            Id = subscription.Id,
            OrganizationId = subscription.OrganizationId,
            PlanCode = plan?.Code ?? "free",
            PlanName = plan?.Name ?? "Free",
            Status = subscription.Status,
            BillingInterval = subscription.BillingInterval,
            PaymentProvider = subscription.PaymentProvider,
            SeatCount = subscription.SeatCount,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            Quotas = quota is null
                ? new QuotaLimits()
                : new QuotaLimits
                {
                    MaxPods = quota.MaxPods,
                    MaxProviders = quota.MaxProviders,
                    MaxModels = quota.MaxModels,
                    MaxOrganizations = quota.MaxOrganizations,
                    MaxTeamMembers = quota.MaxTeamMembers,
                    MaxApiRequestsPerMonth = quota.MaxApiRequestsPerMonth,
                    MaxConcurrentStreams = quota.MaxConcurrentStreams,
                    MaxStorageGb = quota.MaxStorageGb,
                },
        };
    }
}
