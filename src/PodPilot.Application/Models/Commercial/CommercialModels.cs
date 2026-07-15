using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Commercial;

/// <summary>Checkout session request.</summary>
public sealed class CheckoutSessionRequest
{
    public Guid OrganizationId { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public BillingInterval Interval { get; init; }
    public int SeatCount { get; init; } = 1;
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
}

/// <summary>Checkout session result.</summary>
public sealed class CheckoutSessionResult
{
    public string SessionId { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;
    public PaymentProviderKind Provider { get; init; }
}

/// <summary>Start checkout from API.</summary>
public sealed class StartCheckoutRequest
{
    public string PlanCode { get; init; } = "pro";
    public BillingInterval Interval { get; init; } = BillingInterval.Monthly;
    public int SeatCount { get; init; } = 1;
    public PaymentProviderKind Provider { get; init; } = PaymentProviderKind.Stripe;
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
}

/// <summary>Subscription info DTO.</summary>
public sealed class OrganizationSubscriptionInfo
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string PlanCode { get; init; } = "free";
    public string PlanName { get; init; } = "Free";
    public SubscriptionStatus Status { get; init; }
    public BillingInterval BillingInterval { get; init; }
    public PaymentProviderKind? PaymentProvider { get; init; }
    public int SeatCount { get; init; }
    public DateTime CurrentPeriodStart { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public QuotaLimits Quotas { get; init; } = new();
}

/// <summary>Quota limits.</summary>
public sealed class QuotaLimits
{
    public int MaxPods { get; init; }
    public int MaxProviders { get; init; }
    public int MaxModels { get; init; }
    public int MaxOrganizations { get; init; }
    public int MaxTeamMembers { get; init; }
    public long MaxApiRequestsPerMonth { get; init; }
    public int MaxConcurrentStreams { get; init; }
    public int MaxStorageGb { get; init; }
}

/// <summary>Usage dashboard.</summary>
public sealed class UsageDashboard
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal GpuHours { get; init; }
    public long Requests { get; init; }
    public long Tokens { get; init; }
    public decimal BandwidthGb { get; init; }
    public decimal StorageGb { get; init; }
    public int Organizations { get; init; }
    public int Models { get; init; }
    public int Providers { get; init; }
    public decimal EstimatedMonthlyCostUsd { get; init; }
    public QuotaLimits Quotas { get; init; } = new();
    public decimal RequestsQuotaPercent { get; init; }
}

/// <summary>Invoice info.</summary>
public sealed class InvoiceInfo
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Status { get; init; } = "Draft";
    public decimal SubtotalUsd { get; init; }
    public decimal TaxUsd { get; init; }
    public decimal TotalUsd { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public string LineItemsJson { get; init; } = "[]";
}

/// <summary>License info.</summary>
public sealed class LicenseInfo
{
    public Guid Id { get; init; }
    public string LicenseKeyPrefix { get; init; } = string.Empty;
    public LicenseEdition Edition { get; init; }
    public LicenseDeploymentMode DeploymentMode { get; init; }
    public bool IsActivated { get; init; }
    public bool IsValid { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int MaxSeats { get; init; }
    public DateTime? LastValidatedAt { get; init; }
}

/// <summary>Issued license with plaintext key shown once.</summary>
public sealed class IssuedLicense
{
    public LicenseInfo Info { get; init; } = new();
    public string LicenseKey { get; init; } = string.Empty;
}

/// <summary>Issue license request.</summary>
public sealed class IssueLicenseRequest
{
    public Guid? OrganizationId { get; init; }
    public LicenseEdition Edition { get; init; } = LicenseEdition.Professional;
    public LicenseDeploymentMode DeploymentMode { get; init; } = LicenseDeploymentMode.Online;
    public int MaxSeats { get; init; } = 5;
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>Onboarding status.</summary>
public sealed class OnboardingStatus
{
    public OnboardingStep CurrentStep { get; init; }
    public IReadOnlyList<OnboardingStep> CompletedSteps { get; init; } = [];
    public bool IsDismissed { get; init; }
    public bool IsComplete { get; init; }
}

/// <summary>Telemetry preference.</summary>
public sealed class TelemetryPreferenceInfo
{
    public bool OptIn { get; init; }
    public bool CrashReports { get; init; } = true;
    public bool PerformanceMetrics { get; init; } = true;
    public bool FeatureUsage { get; init; } = true;
    public bool HealthReports { get; init; } = true;
}

/// <summary>Backup job info.</summary>
public sealed class BackupJobInfo
{
    public Guid Id { get; init; }
    public Guid? OrganizationId { get; init; }
    public string BackupType { get; init; } = "Database";
    public string Status { get; init; } = "Pending";
    public string? StorageLocator { get; init; }
    public long? SizeBytes { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsScheduled { get; init; }
}

/// <summary>Release status.</summary>
public sealed class ReleaseStatus
{
    public string CurrentVersion { get; init; } = "1.0.0";
    public string LatestVersion { get; init; } = "1.0.0";
    public bool UpdateAvailable { get; init; }
    public string? ReleaseNotes { get; init; }
    public string? DownloadUrl { get; init; }
    public string Channel { get; init; } = "stable";
}

/// <summary>Plan catalog item.</summary>
public sealed class PlanCatalogItem
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Tier { get; init; } = string.Empty;
    public string PricingModel { get; init; } = string.Empty;
    public decimal MonthlyPriceUsd { get; init; }
    public decimal YearlyPriceUsd { get; init; }
    public decimal SeatPriceUsd { get; init; }
    public int IncludedSeats { get; init; }
    public string? Description { get; init; }
    public QuotaLimits Quotas { get; init; } = new();
}

/// <summary>Commercial dashboard.</summary>
public sealed class CommercialDashboard
{
    public OrganizationSubscriptionInfo Subscription { get; init; } = new();
    public UsageDashboard Usage { get; init; } = new();
    public LicenseInfo License { get; init; } = new();
    public ReleaseStatus Release { get; init; } = new();
    public decimal EstimatedMonthlyCostUsd { get; init; }
    public decimal RemainingRequestQuotaPercent { get; init; }
}
