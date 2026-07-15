using PodPilot.Application.Models.Commercial;
using PodPilot.Contracts.Commercial;

namespace PodPilot.Application.Commercial;

/// <summary>
/// Maps commercial application models to contracts.
/// </summary>
internal static class CommercialMapper
{
    public static PlanResponse ToPlanResponse(PlanCatalogItem item) =>
        new()
        {
            Code = item.Code,
            Name = item.Name,
            Tier = item.Tier,
            PricingModel = item.PricingModel,
            MonthlyPriceUsd = item.MonthlyPriceUsd,
            YearlyPriceUsd = item.YearlyPriceUsd,
            SeatPriceUsd = item.SeatPriceUsd,
            IncludedSeats = item.IncludedSeats,
            Description = item.Description,
            Quotas = ToQuotaResponse(item.Quotas),
        };

    public static SubscriptionResponse ToSubscriptionResponse(OrganizationSubscriptionInfo info) =>
        new()
        {
            Id = info.Id,
            PlanCode = info.PlanCode,
            PlanName = info.PlanName,
            Status = info.Status.ToString(),
            BillingInterval = info.BillingInterval.ToString(),
            PaymentProvider = info.PaymentProvider?.ToString(),
            SeatCount = info.SeatCount,
            CurrentPeriodStart = info.CurrentPeriodStart,
            CurrentPeriodEnd = info.CurrentPeriodEnd,
            CancelAtPeriodEnd = info.CancelAtPeriodEnd,
            Quotas = ToQuotaResponse(info.Quotas),
        };

    public static CheckoutSessionResponse ToCheckoutResponse(CheckoutSessionResult result) =>
        new()
        {
            SessionId = result.SessionId,
            CheckoutUrl = result.CheckoutUrl,
            Provider = result.Provider.ToString(),
        };

    public static UsageResponse ToUsageResponse(UsageDashboard usage) =>
        new()
        {
            PeriodStart = usage.PeriodStart,
            PeriodEnd = usage.PeriodEnd,
            GpuHours = usage.GpuHours,
            Requests = usage.Requests,
            Tokens = usage.Tokens,
            BandwidthGb = usage.BandwidthGb,
            StorageGb = usage.StorageGb,
            Organizations = usage.Organizations,
            Models = usage.Models,
            Providers = usage.Providers,
            EstimatedMonthlyCostUsd = usage.EstimatedMonthlyCostUsd,
            Quotas = ToQuotaResponse(usage.Quotas),
            RequestsQuotaPercent = usage.RequestsQuotaPercent,
        };

    public static InvoiceResponse ToInvoiceResponse(InvoiceInfo invoice) =>
        new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Status = invoice.Status,
            SubtotalUsd = invoice.SubtotalUsd,
            TaxUsd = invoice.TaxUsd,
            TotalUsd = invoice.TotalUsd,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd,
            LineItemsJson = invoice.LineItemsJson,
        };

    public static LicenseResponse ToLicenseResponse(LicenseInfo license) =>
        new()
        {
            Id = license.Id,
            LicenseKeyPrefix = license.LicenseKeyPrefix,
            Edition = license.Edition.ToString(),
            DeploymentMode = license.DeploymentMode.ToString(),
            IsActivated = license.IsActivated,
            IsValid = license.IsValid,
            ExpiresAt = license.ExpiresAt,
            MaxSeats = license.MaxSeats,
            LastValidatedAt = license.LastValidatedAt,
        };

    public static IssuedLicenseResponse ToIssuedLicenseResponse(IssuedLicense issued) =>
        new()
        {
            License = ToLicenseResponse(issued.Info),
            LicenseKey = issued.LicenseKey,
        };

    public static OnboardingResponse ToOnboardingResponse(OnboardingStatus status) =>
        new()
        {
            CurrentStep = status.CurrentStep.ToString(),
            CompletedSteps = status.CompletedSteps.Select(s => s.ToString()).ToList(),
            IsDismissed = status.IsDismissed,
            IsComplete = status.IsComplete,
        };

    public static TelemetryPreferenceResponse ToTelemetryResponse(TelemetryPreferenceInfo preference) =>
        new()
        {
            OptIn = preference.OptIn,
            CrashReports = preference.CrashReports,
            PerformanceMetrics = preference.PerformanceMetrics,
            FeatureUsage = preference.FeatureUsage,
            HealthReports = preference.HealthReports,
        };

    public static BackupJobResponse ToBackupResponse(BackupJobInfo job) =>
        new()
        {
            Id = job.Id,
            BackupType = job.BackupType,
            Status = job.Status,
            StorageLocator = job.StorageLocator,
            SizeBytes = job.SizeBytes,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage,
            IsScheduled = job.IsScheduled,
        };

    public static ReleaseStatusResponse ToReleaseResponse(ReleaseStatus status) =>
        new()
        {
            CurrentVersion = status.CurrentVersion,
            LatestVersion = status.LatestVersion,
            UpdateAvailable = status.UpdateAvailable,
            ReleaseNotes = status.ReleaseNotes,
            DownloadUrl = status.DownloadUrl,
            Channel = status.Channel,
        };

    public static CommercialDashboardResponse ToDashboardResponse(CommercialDashboard dashboard) =>
        new()
        {
            Subscription = ToSubscriptionResponse(dashboard.Subscription),
            Usage = ToUsageResponse(dashboard.Usage),
            License = ToLicenseResponse(dashboard.License),
            Release = ToReleaseResponse(dashboard.Release),
            EstimatedMonthlyCostUsd = dashboard.EstimatedMonthlyCostUsd,
            RemainingRequestQuotaPercent = dashboard.RemainingRequestQuotaPercent,
        };

    public static QuotaResponse ToQuotaResponse(QuotaLimits quotas) =>
        new()
        {
            MaxPods = quotas.MaxPods,
            MaxProviders = quotas.MaxProviders,
            MaxModels = quotas.MaxModels,
            MaxOrganizations = quotas.MaxOrganizations,
            MaxTeamMembers = quotas.MaxTeamMembers,
            MaxApiRequestsPerMonth = quotas.MaxApiRequestsPerMonth,
            MaxConcurrentStreams = quotas.MaxConcurrentStreams,
            MaxStorageGb = quotas.MaxStorageGb,
        };
}
