using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Catalog subscription plan definition.
/// </summary>
public class SubscriptionPlan : Common.AuditableEntity
{
    /// <summary>Gets or sets the plan code (free, pro, team, enterprise).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the tier.</summary>
    public SubscriptionPlanTier Tier { get; set; }

    /// <summary>Gets or sets the pricing model.</summary>
    public PricingModel PricingModel { get; set; } = PricingModel.Hybrid;

    /// <summary>Gets or sets monthly price USD.</summary>
    public decimal MonthlyPriceUsd { get; set; }

    /// <summary>Gets or sets yearly price USD.</summary>
    public decimal YearlyPriceUsd { get; set; }

    /// <summary>Gets or sets per-seat monthly price USD.</summary>
    public decimal SeatPriceUsd { get; set; }

    /// <summary>Gets or sets included seats.</summary>
    public int IncludedSeats { get; set; } = 1;

    /// <summary>Gets or sets Stripe price id for monthly.</summary>
    public string? StripeMonthlyPriceId { get; set; }

    /// <summary>Gets or sets Stripe price id for yearly.</summary>
    public string? StripeYearlyPriceId { get; set; }

    /// <summary>Gets or sets Razorpay plan id for monthly.</summary>
    public string? RazorpayMonthlyPlanId { get; set; }

    /// <summary>Gets or sets Razorpay plan id for yearly.</summary>
    public string? RazorpayYearlyPlanId { get; set; }

    /// <summary>Gets or sets whether the plan is publicly listed.</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>Gets or sets a description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets quota definition for this plan.</summary>
    public PlanQuota? Quota { get; set; }
}

/// <summary>
/// Quotas attached to a subscription plan.
/// </summary>
public class PlanQuota : Common.AuditableEntity
{
    /// <summary>Gets or sets the plan id.</summary>
    public Guid SubscriptionPlanId { get; set; }

    /// <summary>Gets or sets maximum pods.</summary>
    public int MaxPods { get; set; }

    /// <summary>Gets or sets maximum providers.</summary>
    public int MaxProviders { get; set; }

    /// <summary>Gets or sets maximum models.</summary>
    public int MaxModels { get; set; }

    /// <summary>Gets or sets maximum organizations.</summary>
    public int MaxOrganizations { get; set; }

    /// <summary>Gets or sets maximum team members.</summary>
    public int MaxTeamMembers { get; set; }

    /// <summary>Gets or sets maximum API requests per month.</summary>
    public long MaxApiRequestsPerMonth { get; set; }

    /// <summary>Gets or sets maximum concurrent streams.</summary>
    public int MaxConcurrentStreams { get; set; }

    /// <summary>Gets or sets maximum storage GB.</summary>
    public int MaxStorageGb { get; set; }

    /// <summary>Gets the plan.</summary>
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}

/// <summary>
/// Organization subscription instance.
/// </summary>
public class OrganizationSubscription : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the plan id.</summary>
    public Guid SubscriptionPlanId { get; set; }

    /// <summary>Gets or sets status.</summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    /// <summary>Gets or sets billing interval.</summary>
    public BillingInterval BillingInterval { get; set; } = BillingInterval.Monthly;

    /// <summary>Gets or sets payment provider.</summary>
    public PaymentProviderKind? PaymentProvider { get; set; }

    /// <summary>Gets or sets external customer id.</summary>
    public string? ExternalCustomerId { get; set; }

    /// <summary>Gets or sets external subscription id.</summary>
    public string? ExternalSubscriptionId { get; set; }

    /// <summary>Gets or sets seat count.</summary>
    public int SeatCount { get; set; } = 1;

    /// <summary>Gets or sets current period start.</summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>Gets or sets current period end.</summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>Gets or sets cancel-at-period-end flag.</summary>
    public bool CancelAtPeriodEnd { get; set; }

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>Gets the plan.</summary>
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}

/// <summary>
/// Metered usage aggregate for an organization period.
/// </summary>
public class UsageRecord : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the metric kind.</summary>
    public UsageMetricKind MetricKind { get; set; }

    /// <summary>Gets or sets the quantity.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Gets or sets the period start.</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Gets or sets the period end.</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Gets or sets estimated cost USD.</summary>
    public decimal EstimatedCostUsd { get; set; }
}

/// <summary>
/// Generated invoice for an organization.
/// </summary>
public class Invoice : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets invoice number.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets status (Draft, Open, Paid, Void).</summary>
    public string Status { get; set; } = "Draft";

    /// <summary>Gets or sets currency.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Gets or sets subtotal.</summary>
    public decimal SubtotalUsd { get; set; }

    /// <summary>Gets or sets tax.</summary>
    public decimal TaxUsd { get; set; }

    /// <summary>Gets or sets total.</summary>
    public decimal TotalUsd { get; set; }

    /// <summary>Gets or sets period start.</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Gets or sets period end.</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Gets or sets line items JSON.</summary>
    public string LineItemsJson { get; set; } = "[]";

    /// <summary>Gets or sets external invoice id.</summary>
    public string? ExternalInvoiceId { get; set; }

    /// <summary>Gets or sets payment provider.</summary>
    public PaymentProviderKind? PaymentProvider { get; set; }

    /// <summary>Gets or sets paid at.</summary>
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// Product license for self-hosted / hosted editions.
/// </summary>
public class ProductLicense : Common.AuditableEntity
{
    /// <summary>Gets or sets organization id (nullable for pre-activation).</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets license key (hashed at rest for online keys; offline stores sealed blob).</summary>
    public string LicenseKeyHash { get; set; } = string.Empty;

    /// <summary>Gets or sets license key prefix for display.</summary>
    public string LicenseKeyPrefix { get; set; } = string.Empty;

    /// <summary>Gets or sets edition.</summary>
    public LicenseEdition Edition { get; set; } = LicenseEdition.Community;

    /// <summary>Gets or sets deployment mode.</summary>
    public LicenseDeploymentMode DeploymentMode { get; set; } = LicenseDeploymentMode.Online;

    /// <summary>Gets or sets whether activated.</summary>
    public bool IsActivated { get; set; }

    /// <summary>Gets or sets activated at.</summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>Gets or sets expires at (null = perpetual).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Gets or sets max seats.</summary>
    public int MaxSeats { get; set; } = 1;

    /// <summary>Gets or sets encrypted/sealed payload for offline validation.</summary>
    public string? EncryptedPayload { get; set; }

    /// <summary>Gets or sets last validated at.</summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>Gets or sets whether valid.</summary>
    public bool IsValid { get; set; } = true;
}

/// <summary>
/// Organization onboarding progress.
/// </summary>
public class OnboardingProgress : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the current step.</summary>
    public OnboardingStep CurrentStep { get; set; } = OnboardingStep.CreateOrganization;

    /// <summary>Gets or sets completed steps JSON array.</summary>
    public string CompletedStepsJson { get; set; } = "[]";

    /// <summary>Gets or sets whether onboarding is dismissed.</summary>
    public bool IsDismissed { get; set; }

    /// <summary>Gets or sets completed at.</summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Opt-in telemetry preference for an organization.
/// </summary>
public class TelemetryPreference : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets whether anonymous telemetry is enabled.</summary>
    public bool OptIn { get; set; }

    /// <summary>Gets or sets crash reports enabled.</summary>
    public bool CrashReports { get; set; } = true;

    /// <summary>Gets or sets performance metrics enabled.</summary>
    public bool PerformanceMetrics { get; set; } = true;

    /// <summary>Gets or sets feature usage enabled.</summary>
    public bool FeatureUsage { get; set; } = true;

    /// <summary>Gets or sets health reports enabled.</summary>
    public bool HealthReports { get; set; } = true;
}

/// <summary>
/// Backup job record.
/// </summary>
public class BackupJob : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id (null = platform).</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets backup type (Database, Configuration).</summary>
    public string BackupType { get; set; } = "Database";

    /// <summary>Gets or sets status.</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Gets or sets storage path or locator.</summary>
    public string? StorageLocator { get; set; }

    /// <summary>Gets or sets size bytes.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>Gets or sets started at.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Gets or sets completed at.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets whether scheduled.</summary>
    public bool IsScheduled { get; set; }
}

/// <summary>
/// Platform release channel version metadata.
/// </summary>
public class PlatformRelease : Common.AuditableEntity
{
    /// <summary>Gets or sets semantic version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets channel (stable, beta).</summary>
    public string Channel { get; set; } = "stable";

    /// <summary>Gets or sets release notes.</summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>Gets or sets published at.</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>Gets or sets download URL for CLI/binaries.</summary>
    public string? DownloadUrl { get; set; }

    /// <summary>Gets or sets whether this is the latest stable.</summary>
    public bool IsLatest { get; set; }
}
