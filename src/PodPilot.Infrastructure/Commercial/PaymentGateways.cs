using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Resolves payment gateways by provider kind.
/// </summary>
public sealed class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> gateways;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentGatewayFactory"/> class.
    /// </summary>
    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways) =>
        this.gateways = gateways;

    /// <inheritdoc />
    public IPaymentGateway Get(PaymentProviderKind kind) =>
        gateways.FirstOrDefault(g => g.ProviderKind == kind)
        ?? throw new InvalidOperationException($"Payment gateway '{kind}' is not registered.");
}

/// <summary>
/// Stripe Checkout Sessions integration with local fallback.
/// </summary>
public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment environment;
    private readonly IApplicationDbContext db;
    private readonly ICommercialNotificationService notifications;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripePaymentGateway"/> class.
    /// </summary>
    public StripePaymentGateway(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHostEnvironment environment,
        IApplicationDbContext db,
        ICommercialNotificationService notifications)
    {
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
        this.environment = environment;
        this.db = db;
        this.notifications = notifications;
    }

    /// <inheritdoc />
    public PaymentProviderKind ProviderKind => PaymentProviderKind.Stripe;

    /// <inheritdoc />
    public async Task<CheckoutSessionResult> CreateCheckoutAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var secretKey = configuration["Payments:Stripe:SecretKey"];
        if (UseLocalMode(secretKey))
        {
            return await ActivateLocallyAsync(request, cancellationToken);
        }

        var plan = await db.SubscriptionPlans.AsNoTracking()
            .FirstAsync(p => p.Code == request.PlanCode, cancellationToken);
        var priceId = request.Interval == BillingInterval.Yearly
            ? plan.StripeYearlyPriceId
            : plan.StripeMonthlyPriceId;

        var client = httpClientFactory.CreateClient(nameof(StripePaymentGateway));
        using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
        message.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:")));

        var form = new Dictionary<string, string>
        {
            ["mode"] = "subscription",
            ["success_url"] = request.SuccessUrl,
            ["cancel_url"] = request.CancelUrl,
            ["customer_email"] = request.CustomerEmail,
            ["client_reference_id"] = request.OrganizationId.ToString(),
            ["metadata[organization_id]"] = request.OrganizationId.ToString(),
            ["metadata[plan_code]"] = request.PlanCode,
            ["line_items[0][quantity]"] = Math.Max(1, request.SeatCount).ToString(),
        };

        if (!string.IsNullOrWhiteSpace(priceId))
        {
            form["line_items[0][price]"] = priceId;
        }
        else
        {
            form["line_items[0][price_data][currency]"] = "usd";
            form["line_items[0][price_data][product_data][name]"] = plan.Name;
            form["line_items[0][price_data][unit_amount]"] = (
                (request.Interval == BillingInterval.Yearly ? plan.YearlyPriceUsd : plan.MonthlyPriceUsd) * 100m)
                .ToString("0");
            form["line_items[0][price_data][recurring][interval]"] =
                request.Interval == BillingInterval.Yearly ? "year" : "month";
        }

        message.Content = new FormUrlEncodedContent(form);
        using var response = await client.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Stripe checkout failed: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var sessionId = doc.RootElement.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N");
        var url = doc.RootElement.GetProperty("url").GetString()
            ?? $"https://checkout.stripe.com/c/pay/{sessionId}";

        await MarkIncompleteAsync(request, sessionId, cancellationToken);

        return new CheckoutSessionResult
        {
            SessionId = sessionId,
            CheckoutUrl = url,
            Provider = PaymentProviderKind.Stripe,
        };
    }

    /// <inheritdoc />
    public async Task CancelSubscriptionAsync(
        string externalSubscriptionId,
        bool atPeriodEnd,
        CancellationToken cancellationToken = default)
    {
        var secretKey = configuration["Payments:Stripe:SecretKey"];
        if (UseLocalMode(secretKey) || string.IsNullOrWhiteSpace(externalSubscriptionId))
        {
            return;
        }

        var client = httpClientFactory.CreateClient(nameof(StripePaymentGateway));
        var path = atPeriodEnd
            ? $"https://api.stripe.com/v1/subscriptions/{externalSubscriptionId}"
            : $"https://api.stripe.com/v1/subscriptions/{externalSubscriptionId}";

        using var message = new HttpRequestMessage(
            atPeriodEnd ? HttpMethod.Post : HttpMethod.Delete,
            path);
        message.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:")));

        if (atPeriodEnd)
        {
            message.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["cancel_at_period_end"] = "true",
            });
        }

        using var response = await client.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Stripe cancel failed: {body}");
        }
    }

    /// <inheritdoc />
    public async Task HandleWebhookAsync(string payload, string? signatureHeader, CancellationToken cancellationToken = default)
    {
        _ = signatureHeader;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("type", out var typeEl))
        {
            return;
        }

        var type = typeEl.GetString();
        if (type is not "checkout.session.completed"
            and not "customer.subscription.updated"
            and not "invoice.paid")
        {
            return;
        }

        if (!doc.RootElement.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("object", out var obj))
        {
            return;
        }

        Guid? organizationId = null;
        if (obj.TryGetProperty("client_reference_id", out var refId) &&
            Guid.TryParse(refId.GetString(), out var parsedRef))
        {
            organizationId = parsedRef;
        }
        else if (obj.TryGetProperty("metadata", out var meta) &&
                 meta.TryGetProperty("organization_id", out var orgMeta) &&
                 Guid.TryParse(orgMeta.GetString(), out var parsedOrg))
        {
            organizationId = parsedOrg;
        }

        if (!organizationId.HasValue)
        {
            return;
        }

        var subscription = await db.OrganizationSubscriptions
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId.Value, cancellationToken);
        if (subscription is null)
        {
            return;
        }

        string? planCode = null;
        if (obj.TryGetProperty("metadata", out var metadata) &&
            metadata.TryGetProperty("plan_code", out var planEl))
        {
            planCode = planEl.GetString();
        }

        if (!string.IsNullOrWhiteSpace(planCode))
        {
            var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == planCode, cancellationToken);
            if (plan is not null)
            {
                subscription.SubscriptionPlanId = plan.Id;
            }
        }

        subscription.Status = SubscriptionStatus.Active;
        subscription.PaymentProvider = PaymentProviderKind.Stripe;
        if (obj.TryGetProperty("subscription", out var subEl))
        {
            subscription.ExternalSubscriptionId = subEl.GetString();
        }
        else if (obj.TryGetProperty("id", out var idEl) && type == "customer.subscription.updated")
        {
            subscription.ExternalSubscriptionId = idEl.GetString();
        }

        subscription.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await notifications.NotifySubscriptionChangedAsync(
            organizationId.Value,
            subscription.Status.ToString(),
            cancellationToken);
    }

    private async Task<CheckoutSessionResult> ActivateLocallyAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var plan = await db.SubscriptionPlans.FirstAsync(p => p.Code == request.PlanCode, cancellationToken);
        var subscription = await db.OrganizationSubscriptions
            .FirstAsync(s => s.OrganizationId == request.OrganizationId, cancellationToken);

        var now = DateTime.UtcNow;
        subscription.SubscriptionPlanId = plan.Id;
        subscription.Status = SubscriptionStatus.Active;
        subscription.BillingInterval = request.Interval;
        subscription.SeatCount = Math.Max(1, request.SeatCount);
        subscription.PaymentProvider = PaymentProviderKind.Stripe;
        subscription.ExternalSubscriptionId = $"local_sub_{sessionId}";
        subscription.ExternalCustomerId = $"local_cus_{request.OrganizationId:N}";
        subscription.CurrentPeriodStart = now;
        subscription.CurrentPeriodEnd = request.Interval == BillingInterval.Yearly
            ? now.AddYears(1)
            : now.AddMonths(1);
        subscription.CancelAtPeriodEnd = false;
        subscription.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await notifications.NotifySubscriptionChangedAsync(
            request.OrganizationId,
            subscription.Status.ToString(),
            cancellationToken);

        return new CheckoutSessionResult
        {
            SessionId = sessionId,
            CheckoutUrl = $"https://checkout.podpilot.local/stripe/{sessionId}",
            Provider = PaymentProviderKind.Stripe,
        };
    }

    private async Task MarkIncompleteAsync(
        CheckoutSessionRequest request,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var subscription = await db.OrganizationSubscriptions
            .FirstOrDefaultAsync(s => s.OrganizationId == request.OrganizationId, cancellationToken);
        if (subscription is null)
        {
            return;
        }

        subscription.Status = SubscriptionStatus.Incomplete;
        subscription.PaymentProvider = PaymentProviderKind.Stripe;
        subscription.ExternalSubscriptionId = sessionId;
        subscription.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private bool UseLocalMode(string? secretKey) =>
        environment.IsEnvironment("Testing") || string.IsNullOrWhiteSpace(secretKey);
}

/// <summary>
/// Razorpay subscriptions integration with local fallback.
/// </summary>
public sealed class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment environment;
    private readonly IApplicationDbContext db;
    private readonly ICommercialNotificationService notifications;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorpayPaymentGateway"/> class.
    /// </summary>
    public RazorpayPaymentGateway(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHostEnvironment environment,
        IApplicationDbContext db,
        ICommercialNotificationService notifications)
    {
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
        this.environment = environment;
        this.db = db;
        this.notifications = notifications;
    }

    /// <inheritdoc />
    public PaymentProviderKind ProviderKind => PaymentProviderKind.Razorpay;

    /// <inheritdoc />
    public async Task<CheckoutSessionResult> CreateCheckoutAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var keyId = configuration["Payments:Razorpay:KeyId"];
        var keySecret = configuration["Payments:Razorpay:KeySecret"];
        if (UseLocalMode(keyId, keySecret))
        {
            return await ActivateLocallyAsync(request, cancellationToken);
        }

        var plan = await db.SubscriptionPlans.AsNoTracking()
            .FirstAsync(p => p.Code == request.PlanCode, cancellationToken);
        var razorpayPlanId = request.Interval == BillingInterval.Yearly
            ? plan.RazorpayYearlyPlanId
            : plan.RazorpayMonthlyPlanId;

        var client = httpClientFactory.CreateClient(nameof(RazorpayPaymentGateway));
        using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/subscriptions");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var payload = new Dictionary<string, object?>
        {
            ["plan_id"] = razorpayPlanId ?? $"plan_{plan.Code}_{request.Interval.ToString().ToLowerInvariant()}",
            ["total_count"] = request.Interval == BillingInterval.Yearly ? 10 : 12,
            ["quantity"] = Math.Max(1, request.SeatCount),
            ["customer_notify"] = 1,
            ["notes"] = new Dictionary<string, string>
            {
                ["organization_id"] = request.OrganizationId.ToString(),
                ["plan_code"] = request.PlanCode,
            },
        };

        message.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay checkout failed: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var sessionId = doc.RootElement.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N");
        var shortUrl = doc.RootElement.TryGetProperty("short_url", out var urlEl)
            ? urlEl.GetString()
            : null;

        var subscription = await db.OrganizationSubscriptions
            .FirstAsync(s => s.OrganizationId == request.OrganizationId, cancellationToken);
        subscription.Status = SubscriptionStatus.Incomplete;
        subscription.PaymentProvider = PaymentProviderKind.Razorpay;
        subscription.ExternalSubscriptionId = sessionId;
        subscription.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new CheckoutSessionResult
        {
            SessionId = sessionId,
            CheckoutUrl = shortUrl ?? $"https://rzp.io/i/{sessionId}",
            Provider = PaymentProviderKind.Razorpay,
        };
    }

    /// <inheritdoc />
    public async Task CancelSubscriptionAsync(
        string externalSubscriptionId,
        bool atPeriodEnd,
        CancellationToken cancellationToken = default)
    {
        var keyId = configuration["Payments:Razorpay:KeyId"];
        var keySecret = configuration["Payments:Razorpay:KeySecret"];
        if (UseLocalMode(keyId, keySecret) || string.IsNullOrWhiteSpace(externalSubscriptionId))
        {
            return;
        }

        var client = httpClientFactory.CreateClient(nameof(RazorpayPaymentGateway));
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://api.razorpay.com/v1/subscriptions/{externalSubscriptionId}/cancel");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        message.Content = new StringContent(
            JsonSerializer.Serialize(new { cancel_at_cycle_end = atPeriodEnd }),
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Razorpay cancel failed: {body}");
        }
    }

    /// <inheritdoc />
    public async Task HandleWebhookAsync(string payload, string? signatureHeader, CancellationToken cancellationToken = default)
    {
        _ = signatureHeader;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("event", out var eventEl))
        {
            return;
        }

        var eventName = eventEl.GetString();
        if (eventName is not "subscription.activated"
            and not "subscription.charged"
            and not "payment.captured")
        {
            return;
        }

        if (!doc.RootElement.TryGetProperty("payload", out var payloadEl))
        {
            return;
        }

        JsonElement entity = default;
        if (payloadEl.TryGetProperty("subscription", out var subWrap) &&
            subWrap.TryGetProperty("entity", out var subEntity))
        {
            entity = subEntity;
        }
        else if (payloadEl.TryGetProperty("payment", out var payWrap) &&
                 payWrap.TryGetProperty("entity", out var payEntity))
        {
            entity = payEntity;
        }
        else
        {
            return;
        }

        Guid? organizationId = null;
        string? planCode = null;
        if (entity.TryGetProperty("notes", out var notes))
        {
            if (notes.TryGetProperty("organization_id", out var orgEl) &&
                Guid.TryParse(orgEl.GetString(), out var parsedOrg))
            {
                organizationId = parsedOrg;
            }

            if (notes.TryGetProperty("plan_code", out var planEl))
            {
                planCode = planEl.GetString();
            }
        }

        if (!organizationId.HasValue)
        {
            return;
        }

        var subscription = await db.OrganizationSubscriptions
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId.Value, cancellationToken);
        if (subscription is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(planCode))
        {
            var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == planCode, cancellationToken);
            if (plan is not null)
            {
                subscription.SubscriptionPlanId = plan.Id;
            }
        }

        subscription.Status = SubscriptionStatus.Active;
        subscription.PaymentProvider = PaymentProviderKind.Razorpay;
        if (entity.TryGetProperty("id", out var idEl))
        {
            subscription.ExternalSubscriptionId = idEl.GetString();
        }

        subscription.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await notifications.NotifySubscriptionChangedAsync(
            organizationId.Value,
            subscription.Status.ToString(),
            cancellationToken);
    }

    private async Task<CheckoutSessionResult> ActivateLocallyAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var plan = await db.SubscriptionPlans.FirstAsync(p => p.Code == request.PlanCode, cancellationToken);
        var subscription = await db.OrganizationSubscriptions
            .FirstAsync(s => s.OrganizationId == request.OrganizationId, cancellationToken);

        var now = DateTime.UtcNow;
        subscription.SubscriptionPlanId = plan.Id;
        subscription.Status = SubscriptionStatus.Active;
        subscription.BillingInterval = request.Interval;
        subscription.SeatCount = Math.Max(1, request.SeatCount);
        subscription.PaymentProvider = PaymentProviderKind.Razorpay;
        subscription.ExternalSubscriptionId = $"local_rzp_{sessionId}";
        subscription.ExternalCustomerId = $"local_rzp_cus_{request.OrganizationId:N}";
        subscription.CurrentPeriodStart = now;
        subscription.CurrentPeriodEnd = request.Interval == BillingInterval.Yearly
            ? now.AddYears(1)
            : now.AddMonths(1);
        subscription.CancelAtPeriodEnd = false;
        subscription.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await notifications.NotifySubscriptionChangedAsync(
            request.OrganizationId,
            subscription.Status.ToString(),
            cancellationToken);

        return new CheckoutSessionResult
        {
            SessionId = sessionId,
            CheckoutUrl = $"https://checkout.podpilot.local/razorpay/{sessionId}",
            Provider = PaymentProviderKind.Razorpay,
        };
    }

    private bool UseLocalMode(string? keyId, string? keySecret) =>
        environment.IsEnvironment("Testing")
        || string.IsNullOrWhiteSpace(keyId)
        || string.IsNullOrWhiteSpace(keySecret);
}
