using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Security;

/// <summary>Request to store a secret in a backend.</summary>
public sealed class SecretStoreRequest
{
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Plaintext { get; init; } = string.Empty;
    public string? ExistingLocator { get; init; }
}

/// <summary>Create secret application request.</summary>
public sealed class CreateSecretRequest
{
    public string Name { get; init; } = string.Empty;
    public SecretKind SecretKind { get; init; }
    public SecretBackendKind BackendKind { get; init; } = SecretBackendKind.LocalEncrypted;
    public string Plaintext { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>Update/rotate secret application request.</summary>
public sealed class UpdateSecretRequest
{
    public string? Name { get; init; }
    public string? Plaintext { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool? IsEnabled { get; init; }
}

/// <summary>Enterprise audit entry.</summary>
public sealed class EnterpriseAuditEntry
{
    public Guid Id { get; init; }
    public Guid? OrganizationId { get; init; }
    public Guid? UserId { get; init; }
    public string? ActorEmail { get; init; }
    public AuditEventCategory Category { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? MetadataJson { get; init; }
    public string? IpAddress { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime OccurredAt { get; init; }
}

/// <summary>Audit query filters.</summary>
public sealed class AuditQueryRequest
{
    public AuditEventCategory? Category { get; init; }
    public string? EventType { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int Take { get; init; } = 100;
}

/// <summary>Begin SSO challenge.</summary>
public sealed class SsoBeginRequest
{
    public Guid OrganizationId { get; init; }
    public Guid IdentityProviderId { get; init; }
    public string RedirectUri { get; init; } = string.Empty;
}

/// <summary>SSO challenge result.</summary>
public sealed class SsoChallengeResult
{
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}

/// <summary>Complete SSO.</summary>
public sealed class SsoCompleteRequest
{
    public Guid OrganizationId { get; init; }
    public Guid IdentityProviderId { get; init; }
    public string? Code { get; init; }
    public string? State { get; init; }
    public string? SamlResponse { get; init; }
    public string RedirectUri { get; init; } = string.Empty;
}

/// <summary>SSO completion with issued tokens.</summary>
public sealed class SsoCompletionResult
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool RequiresMfa { get; init; }
    public string? MfaChallengeToken { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
}

/// <summary>SCIM user upsert.</summary>
public sealed class ScimUserRequest
{
    public string ExternalId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool Active { get; init; } = true;
    public IReadOnlyList<string> Groups { get; init; } = [];
}

/// <summary>SCIM user result.</summary>
public sealed class ScimUserResult
{
    public Guid UserId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public bool Created { get; init; }
}

/// <summary>SCIM group sync.</summary>
public sealed class ScimGroupRequest
{
    public string ExternalGroupId { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string OrganizationRole { get; init; } = "Viewer";
    public IReadOnlyList<string> MemberExternalIds { get; init; } = [];
}

/// <summary>MFA enrollment result.</summary>
public sealed class MfaEnrollmentResult
{
    public string SharedSecret { get; init; } = string.Empty;
    public string OtpAuthUri { get; init; } = string.Empty;
}

/// <summary>Session tracking request.</summary>
public sealed class SessionTrackRequest
{
    public Guid UserId { get; init; }
    public Guid? OrganizationId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? DeviceFingerprint { get; init; }
    public string? CountryCode { get; init; }
    public bool Succeeded { get; init; } = true;
    public string? FailureReason { get; init; }
}

/// <summary>Active session info.</summary>
public sealed class SessionInfo
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime LastSeenAt { get; init; }
}

/// <summary>Security alert payload.</summary>
public sealed class SecurityAlert
{
    public Guid OrganizationId { get; init; }
    public SecurityAlertType AlertType { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>Compliance status.</summary>
public sealed class ComplianceStatus
{
    public bool GdprEnabled { get; init; }
    public bool Soc2Enabled { get; init; }
    public bool Iso27001Enabled { get; init; }
    public int DataRetentionDays { get; init; }
    public int LogRetentionDays { get; init; }
    public DateTime? LastExportAt { get; init; }
    public DateTime? LastErasureAt { get; init; }
    public string OverallStatus { get; init; } = "Ready";
    public IReadOnlyList<string> ControlChecklist { get; init; } = [];
}

/// <summary>GDPR export payload.</summary>
public sealed class ComplianceExportResult
{
    public string JsonPayload { get; init; } = "{}";
    public DateTime ExportedAt { get; init; }
}

/// <summary>Security dashboard metrics.</summary>
public sealed class SecurityDashboard
{
    public int SecurityScore { get; init; }
    public int ActiveSessions { get; init; }
    public int FailedLogins24h { get; init; }
    public int RecentAuditEvents { get; init; }
    public int SecretCount { get; init; }
    public int ExpiringSecrets { get; init; }
    public double MfaCoveragePercent { get; init; }
    public string ComplianceStatus { get; init; } = "Unknown";
    public IReadOnlyList<EnterpriseAuditEntry> RecentAudits { get; init; } = [];
}
