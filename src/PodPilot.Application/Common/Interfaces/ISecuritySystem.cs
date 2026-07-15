using PodPilot.Application.Models.Security;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Resolves and reads secrets from configured backends. Never returns plaintext via API contracts.
/// </summary>
public interface ISecretProvider
{
    /// <summary>Gets the backend kind.</summary>
    SecretBackendKind BackendKind { get; }

    /// <summary>Stores a secret and returns the backend locator.</summary>
    Task<string> StoreAsync(SecretStoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a secret value for internal use only.</summary>
    Task<string?> GetValueAsync(string backendLocator, CancellationToken cancellationToken = default);

    /// <summary>Rotates a secret value.</summary>
    Task RotateAsync(string backendLocator, string newPlaintext, CancellationToken cancellationToken = default);

    /// <summary>Deletes a secret from the backend.</summary>
    Task DeleteAsync(string backendLocator, CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory for secret providers.
/// </summary>
public interface ISecretProviderFactory
{
    /// <summary>Gets a provider for the given backend.</summary>
    ISecretProvider GetProvider(SecretBackendKind backendKind);
}

/// <summary>
/// Organization secret catalog operations.
/// </summary>
public interface ISecretManager
{
    /// <summary>Creates a secret reference and stores the value.</summary>
    Task<Guid> CreateAsync(Guid organizationId, CreateSecretRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates metadata or rotates the value.</summary>
    Task UpdateAsync(Guid organizationId, Guid secretId, UpdateSecretRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a secret.</summary>
    Task DeleteAsync(Guid organizationId, Guid secretId, CancellationToken cancellationToken = default);

    /// <summary>Reads plaintext for internal services only (audited).</summary>
    Task<string?> ResolveAsync(Guid organizationId, Guid secretId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Enterprise immutable audit writer/reader.
/// </summary>
public interface IEnterpriseAuditService
{
    /// <summary>Appends an immutable audit event.</summary>
    Task AppendAsync(EnterpriseAuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Queries audit events for an organization.</summary>
    Task<IReadOnlyList<EnterpriseAuditEntry>> QueryAsync(
        Guid organizationId,
        AuditQueryRequest query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SSO orchestration for org-configured identity providers.
/// </summary>
public interface ISsoService
{
    /// <summary>Builds an authorization redirect URL.</summary>
    Task<SsoChallengeResult> BeginAsync(SsoBeginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Completes OIDC/OAuth or SAML login and issues tokens.</summary>
    Task<SsoCompletionResult> CompleteAsync(SsoCompleteRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// SCIM 2.0 user/group provisioning.
/// </summary>
public interface IScimProvisioningService
{
    /// <summary>Creates or updates a user from a SCIM payload.</summary>
    Task<ScimUserResult> UpsertUserAsync(Guid organizationId, ScimUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>Disables a user.</summary>
    Task DisableUserAsync(Guid organizationId, string externalUserId, CancellationToken cancellationToken = default);

    /// <summary>Synchronizes a group membership mapping.</summary>
    Task SyncGroupAsync(Guid organizationId, ScimGroupRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// TOTP MFA enrollment and verification.
/// </summary>
public interface IMfaService
{
    /// <summary>Starts TOTP enrollment and returns otpauth URI.</summary>
    Task<MfaEnrollmentResult> BeginEnrollmentAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Confirms enrollment with a TOTP code.</summary>
    Task ConfirmEnrollmentAsync(Guid userId, string code, CancellationToken cancellationToken = default);

    /// <summary>Verifies a TOTP code for login.</summary>
    Task<bool> VerifyAsync(Guid userId, string code, CancellationToken cancellationToken = default);

    /// <summary>Disables MFA for a user.</summary>
    Task DisableAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns whether MFA is enabled.</summary>
    Task<bool> IsEnabledAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Evaluates organization governance and security policies.
/// </summary>
public interface IPolicyEngine
{
    /// <summary>Ensures a provider kind is allowed.</summary>
    Task EnsureProviderAllowedAsync(Guid organizationId, string providerKind, CancellationToken cancellationToken = default);

    /// <summary>Ensures a model is allowed.</summary>
    Task EnsureModelAllowedAsync(Guid organizationId, string modelName, CancellationToken cancellationToken = default);

    /// <summary>Ensures pod count is within limit.</summary>
    Task EnsurePodLimitAsync(Guid organizationId, int proposedRunningPods, CancellationToken cancellationToken = default);

    /// <summary>Ensures queue size is within limit.</summary>
    Task EnsureQueueLimitAsync(Guid organizationId, int proposedQueueSize, CancellationToken cancellationToken = default);

    /// <summary>Ensures IP is allowed.</summary>
    Task EnsureIpAllowedAsync(Guid organizationId, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>Ensures plugin package is allowed.</summary>
    Task EnsurePluginAllowedAsync(Guid organizationId, string packageId, CancellationToken cancellationToken = default);

    /// <summary>Ensures MCP kind is allowed.</summary>
    Task EnsureMcpAllowedAsync(Guid organizationId, string serverKind, CancellationToken cancellationToken = default);
}

/// <summary>
/// Compliance export, erasure, and status.
/// </summary>
public interface IComplianceService
{
    /// <summary>Gets compliance dashboard status.</summary>
    Task<ComplianceStatus> GetStatusAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Exports organization user data (GDPR).</summary>
    Task<ComplianceExportResult> ExportAsync(Guid organizationId, Guid requestingUserId, CancellationToken cancellationToken = default);

    /// <summary>Performs right-to-erasure for a user within the org.</summary>
    Task EraseUserAsync(Guid organizationId, Guid targetUserId, Guid requestingUserId, CancellationToken cancellationToken = default);

    /// <summary>Applies retention policies (purge old audit/logs).</summary>
    Task ApplyRetentionAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Session and device tracking.
/// </summary>
public interface ISessionTracker
{
    /// <summary>Records a successful or failed login session.</summary>
    Task TrackLoginAsync(SessionTrackRequest request, CancellationToken cancellationToken = default);

    /// <summary>Ends a session.</summary>
    Task EndSessionAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Lists active sessions for a user/org.</summary>
    Task<IReadOnlyList<SessionInfo>> ListActiveAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Enforces concurrent session limits.</summary>
    Task EnforceConcurrentLimitAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Raises and broadcasts security alerts.
/// </summary>
public interface ISecurityAlertService
{
    /// <summary>Records and broadcasts a security alert.</summary>
    Task RaiseAsync(SecurityAlert alert, CancellationToken cancellationToken = default);
}

/// <summary>
/// SignalR notifications for security.
/// </summary>
public interface ISecurityNotificationService
{
    /// <summary>Broadcasts a security alert.</summary>
    Task NotifySecurityAlertAsync(Guid organizationId, string alertType, string message, CancellationToken cancellationToken = default);

    /// <summary>Broadcasts an audit event tip.</summary>
    Task NotifyAuditEventAsync(Guid organizationId, string eventType, CancellationToken cancellationToken = default);

    /// <summary>Broadcasts a policy violation.</summary>
    Task NotifyPolicyViolationAsync(Guid organizationId, string message, CancellationToken cancellationToken = default);

    /// <summary>Broadcasts a new login.</summary>
    Task NotifyNewLoginAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Broadcasts provider credential change.</summary>
    Task NotifyCredentialChangeAsync(Guid organizationId, string providerName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Computes security dashboard metrics.
/// </summary>
public interface ISecurityDashboardService
{
    /// <summary>Gets security dashboard for an organization.</summary>
    Task<SecurityDashboard> GetAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Short-lived MFA challenge tokens issued after password validation.
/// </summary>
public interface IMfaChallengeStore
{
    /// <summary>Stores a challenge token for a user.</summary>
    Task StoreAsync(string token, Guid userId, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>Peeks at the user id for a token without consuming it.</summary>
    Task<Guid?> PeekAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Consumes a challenge token and returns the associated user id.</summary>
    Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default);
}
