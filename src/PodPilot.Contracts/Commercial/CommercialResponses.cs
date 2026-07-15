namespace PodPilot.Contracts.Commercial;

/// <summary>Subscription plan catalog response.</summary>
public sealed class PlanResponse
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
    public QuotaResponse Quotas { get; init; } = new();
}

/// <summary>Quota response.</summary>
public sealed class QuotaResponse
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

/// <summary>Organization subscription response.</summary>
public sealed class SubscriptionResponse
{
    public Guid Id { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string BillingInterval { get; init; } = string.Empty;
    public string? PaymentProvider { get; init; }
    public int SeatCount { get; init; }
    public DateTime CurrentPeriodStart { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public QuotaResponse Quotas { get; init; } = new();
}

/// <summary>Start checkout request.</summary>
public sealed class StartCheckoutRequest
{
    public string PlanCode { get; set; } = "pro";
    public string Interval { get; set; } = "Monthly";
    public int SeatCount { get; set; } = 1;
    public string Provider { get; set; } = "Stripe";
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>Checkout session response.</summary>
public sealed class CheckoutSessionResponse
{
    public string SessionId { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}

/// <summary>Usage dashboard response.</summary>
public sealed class UsageResponse
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
    public QuotaResponse Quotas { get; init; } = new();
    public decimal RequestsQuotaPercent { get; init; }
}

/// <summary>Invoice response.</summary>
public sealed class InvoiceResponse
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal SubtotalUsd { get; init; }
    public decimal TaxUsd { get; init; }
    public decimal TotalUsd { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public string LineItemsJson { get; init; } = "[]";
}

/// <summary>License response.</summary>
public sealed class LicenseResponse
{
    public Guid Id { get; init; }
    public string LicenseKeyPrefix { get; init; } = string.Empty;
    public string Edition { get; init; } = string.Empty;
    public string DeploymentMode { get; init; } = string.Empty;
    public bool IsActivated { get; init; }
    public bool IsValid { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int MaxSeats { get; init; }
    public DateTime? LastValidatedAt { get; init; }
}

/// <summary>Activate license request.</summary>
public sealed class ActivateLicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
}

/// <summary>Issue license request.</summary>
public sealed class IssueLicenseRequest
{
    public Guid? OrganizationId { get; set; }

    public string Edition { get; set; } = "Professional";

    public string DeploymentMode { get; set; } = "Online";

    public int MaxSeats { get; set; } = 5;

    public DateTime? ExpiresAt { get; set; }
}

/// <summary>Cancel subscription request.</summary>
public sealed class CancelSubscriptionRequest
{
    public bool AtPeriodEnd { get; set; } = true;
}

/// <summary>Issued license response (key once).</summary>
public sealed class IssuedLicenseResponse
{
    public LicenseResponse License { get; init; } = new();
    public string LicenseKey { get; init; } = string.Empty;
}

/// <summary>Onboarding response.</summary>
public sealed class OnboardingResponse
{
    public string CurrentStep { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSteps { get; init; } = [];
    public bool IsDismissed { get; init; }
    public bool IsComplete { get; init; }
}

/// <summary>Complete onboarding step request.</summary>
public sealed class CompleteOnboardingStepRequest
{
    public string Step { get; set; } = string.Empty;
}

/// <summary>Telemetry preference request/response.</summary>
public sealed class TelemetryPreferenceResponse
{
    public bool OptIn { get; init; }
    public bool CrashReports { get; init; }
    public bool PerformanceMetrics { get; init; }
    public bool FeatureUsage { get; init; }
    public bool HealthReports { get; init; }
}

/// <summary>Backup job response.</summary>
public sealed class BackupJobResponse
{
    public Guid Id { get; init; }
    public string BackupType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? StorageLocator { get; init; }
    public long? SizeBytes { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsScheduled { get; init; }
}

/// <summary>Start backup request.</summary>
public sealed class StartBackupRequest
{
    public string BackupType { get; set; } = "Database";
    public bool Scheduled { get; set; }
}

/// <summary>Release status response.</summary>
public sealed class ReleaseStatusResponse
{
    public string CurrentVersion { get; init; } = string.Empty;
    public string LatestVersion { get; init; } = string.Empty;
    public bool UpdateAvailable { get; init; }
    public string? ReleaseNotes { get; init; }
    public string? DownloadUrl { get; init; }
    public string Channel { get; init; } = "stable";
}

/// <summary>Commercial dashboard response.</summary>
public sealed class CommercialDashboardResponse
{
    public SubscriptionResponse Subscription { get; init; } = new();
    public UsageResponse Usage { get; init; } = new();
    public LicenseResponse License { get; init; } = new();
    public ReleaseStatusResponse Release { get; init; } = new();
    public decimal EstimatedMonthlyCostUsd { get; init; }
    public decimal RemainingRequestQuotaPercent { get; init; }
}

/// <summary>System status response.</summary>
public sealed class SystemStatusResponse
{
    public string Status { get; init; } = "Operational";
    public string Version { get; init; } = string.Empty;
    public bool UpdateAvailable { get; init; }
    public IReadOnlyList<SystemComponentStatus> Components { get; init; } = [];
}

/// <summary>Component status.</summary>
public sealed class SystemComponentStatus
{
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = "Operational";
}
