using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Creates checkout sessions and syncs subscriptions with payment providers.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Gets the provider kind.</summary>
    PaymentProviderKind ProviderKind { get; }

    /// <summary>Creates a checkout / subscription session.</summary>
    Task<CheckoutSessionResult> CreateCheckoutAsync(CheckoutSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels an external subscription.</summary>
    Task CancelSubscriptionAsync(string externalSubscriptionId, bool atPeriodEnd, CancellationToken cancellationToken = default);

    /// <summary>Handles a provider webhook payload.</summary>
    Task HandleWebhookAsync(string payload, string? signatureHeader, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves payment gateways.
/// </summary>
public interface IPaymentGatewayFactory
{
    /// <summary>Gets a gateway by kind.</summary>
    IPaymentGateway Get(PaymentProviderKind kind);
}

/// <summary>
/// Manages organization subscriptions and plan catalog.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>Ensures catalog plans exist.</summary>
    Task EnsureCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the org subscription (creates Free if missing).</summary>
    Task<OrganizationSubscriptionInfo> GetOrCreateAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Starts checkout for an upgrade.</summary>
    Task<CheckoutSessionResult> StartCheckoutAsync(Guid organizationId, StartCheckoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels the organization subscription.</summary>
    Task CancelAsync(Guid organizationId, bool atPeriodEnd, CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregates and records usage for billing.
/// </summary>
public interface IUsageMeteringService
{
    /// <summary>Records a usage increment.</summary>
    Task RecordAsync(Guid organizationId, UsageMetricKind metric, decimal quantity, decimal estimatedCostUsd = 0, CancellationToken cancellationToken = default);

    /// <summary>Builds current-period usage snapshot from live data + records.</summary>
    Task<UsageDashboard> GetUsageAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Generates an invoice for the current (or previous) period.</summary>
    Task<InvoiceInfo> GenerateInvoiceAsync(Guid organizationId, DateTime? periodStart = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Enforces plan quotas.
/// </summary>
public interface IQuotaService
{
    /// <summary>Gets effective quotas for an organization.</summary>
    Task<QuotaLimits> GetLimitsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Ensures creating another pod is allowed.</summary>
    Task EnsureCanCreatePodAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Ensures creating another provider is allowed.</summary>
    Task EnsureCanCreateProviderAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Ensures API request within monthly quota.</summary>
    Task EnsureApiRequestAllowedAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// License activation and validation.
/// </summary>
public interface ILicenseService
{
    /// <summary>Activates a license key for an organization.</summary>
    Task<LicenseInfo> ActivateAsync(Guid organizationId, string licenseKey, CancellationToken cancellationToken = default);

    /// <summary>Validates the current license.</summary>
    Task<LicenseInfo> ValidateAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Issues a new license (admin / self-hosted tooling).</summary>
    Task<IssuedLicense> IssueAsync(IssueLicenseRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Onboarding wizard state.
/// </summary>
public interface IOnboardingService
{
    /// <summary>Gets onboarding status.</summary>
    Task<OnboardingStatus> GetAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Marks a step complete and advances.</summary>
    Task<OnboardingStatus> CompleteStepAsync(Guid organizationId, OnboardingStep step, CancellationToken cancellationToken = default);

    /// <summary>Dismisses onboarding.</summary>
    Task DismissAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Opt-in telemetry collection.
/// </summary>
public interface ITelemetryService
{
    /// <summary>Gets preference.</summary>
    Task<TelemetryPreferenceInfo> GetPreferenceAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Updates preference.</summary>
    Task UpdatePreferenceAsync(Guid organizationId, TelemetryPreferenceInfo preference, CancellationToken cancellationToken = default);

    /// <summary>Records an anonymous event when opted in.</summary>
    Task TrackAsync(Guid organizationId, string eventName, IReadOnlyDictionary<string, string>? properties = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Database and configuration backups.
/// </summary>
public interface IBackupService
{
    /// <summary>Starts a backup job.</summary>
    Task<BackupJobInfo> StartAsync(Guid? organizationId, string backupType, bool scheduled, CancellationToken cancellationToken = default);

    /// <summary>Lists backup jobs.</summary>
    Task<IReadOnlyList<BackupJobInfo>> ListAsync(Guid? organizationId, CancellationToken cancellationToken = default);

    /// <summary>Restores from a completed backup.</summary>
    Task RestoreAsync(Guid backupJobId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Platform version / release checks.
/// </summary>
public interface IReleaseService
{
    /// <summary>Gets current and latest version info.</summary>
    Task<ReleaseStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Ensures default release channel seeded.</summary>
    Task EnsureDefaultReleaseAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Commercial / billing dashboard aggregation.
/// </summary>
public interface ICommercialDashboardService
{
    /// <summary>Gets commercial dashboard for an organization.</summary>
    Task<CommercialDashboard> GetAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// SignalR notifications for billing/commercial.
/// </summary>
public interface ICommercialNotificationService
{
    /// <summary>Notifies subscription changed.</summary>
    Task NotifySubscriptionChangedAsync(Guid organizationId, string status, CancellationToken cancellationToken = default);

    /// <summary>Notifies usage threshold.</summary>
    Task NotifyUsageThresholdAsync(Guid organizationId, string metric, decimal percentUsed, CancellationToken cancellationToken = default);

    /// <summary>Notifies invoice generated.</summary>
    Task NotifyInvoiceGeneratedAsync(Guid organizationId, string invoiceNumber, CancellationToken cancellationToken = default);

    /// <summary>Notifies license updated.</summary>
    Task NotifyLicenseUpdatedAsync(Guid organizationId, string edition, CancellationToken cancellationToken = default);
}
