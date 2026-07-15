namespace PodPilot.Contracts.Security;

/// <summary>Identity provider response (no secrets).</summary>
public sealed class IdentityProviderResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProviderKind { get; init; } = string.Empty;
    public string Protocol { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string? Issuer { get; init; }
    public string? ClientId { get; init; }
    public bool HasClientSecret { get; init; }
    public string Scopes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>Create identity provider request.</summary>
public sealed class CreateIdentityProviderRequest
{
    public string Name { get; set; } = string.Empty;
    public string ProviderKind { get; set; } = "EntraId";
    public string Protocol { get; set; } = "Oidc";
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Issuer { get; set; }
    public string? AuthorizationEndpoint { get; set; }
    public string? TokenEndpoint { get; set; }
    public string? JwksUri { get; set; }
    public string? SamlEntityId { get; set; }
    public string? SamlSsoUrl { get; set; }
    public string? SamlCertificate { get; set; }
    public string Scopes { get; set; } = "openid profile email";
    public bool IsEnabled { get; set; } = true;
}

/// <summary>Begin SSO request.</summary>
public sealed class BeginSsoRequest
{
    public Guid OrganizationId { get; set; }
    public Guid IdentityProviderId { get; set; }
    public string RedirectUri { get; set; } = string.Empty;
}

/// <summary>SSO challenge response.</summary>
public sealed class SsoChallengeResponse
{
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}

/// <summary>Complete SSO request.</summary>
public sealed class CompleteSsoRequest
{
    public Guid OrganizationId { get; set; }
    public Guid IdentityProviderId { get; set; }
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? SamlResponse { get; set; }
    public string RedirectUri { get; set; } = string.Empty;
}

/// <summary>MFA verify / enroll request.</summary>
public sealed class MfaRequest
{
    public string? Code { get; set; }
    public string? MfaToken { get; set; }
    public string Action { get; set; } = "verify";
}

/// <summary>MFA enrollment response.</summary>
public sealed class MfaEnrollmentResponse
{
    public string SharedSecret { get; init; } = string.Empty;
    public string OtpAuthUri { get; init; } = string.Empty;
}

/// <summary>Secret metadata response (never includes plaintext).</summary>
public sealed class SecretResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SecretKind { get; init; } = string.Empty;
    public string BackendKind { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
    public DateTime? LastRotatedAt { get; init; }
    public DateTime? LastAccessedAt { get; init; }
    public bool IsEnabled { get; init; }
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Create secret request.</summary>
public sealed class CreateSecretRequest
{
    public string Name { get; set; } = string.Empty;
    public string SecretKind { get; set; } = "Generic";
    public string BackendKind { get; set; } = "LocalEncrypted";
    public string Value { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>Update secret request.</summary>
public sealed class UpdateSecretRequest
{
    public string? Name { get; set; }
    public string? Value { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>Audit event response.</summary>
public sealed class AuditEventResponse
{
    public Guid Id { get; init; }
    public Guid? OrganizationId { get; init; }
    public Guid? UserId { get; init; }
    public string? ActorEmail { get; init; }
    public string Category { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public DateTime OccurredAt { get; init; }
}

/// <summary>Governance + security policies response.</summary>
public sealed class OrganizationPoliciesResponse
{
    public SecurityPolicyResponse Security { get; init; } = new();
    public GovernancePolicyResponse Governance { get; init; } = new();
}

/// <summary>Security policy DTO.</summary>
public sealed class SecurityPolicyResponse
{
    public int MinPasswordLength { get; init; }
    public bool RequireUppercase { get; init; }
    public bool RequireDigit { get; init; }
    public bool RequireNonAlphanumeric { get; init; }
    public bool RequireMfa { get; init; }
    public int SessionTimeoutMinutes { get; init; }
    public int MaxConcurrentSessions { get; init; }
    public IReadOnlyList<string> IpAllowList { get; init; } = [];
    public IReadOnlyList<string> GeoAllowList { get; init; } = [];
    public int ApiKeyExpirationDays { get; init; }
    public bool EnforceApiKeyRotation { get; init; }
    public int FailedLoginAlertThreshold { get; init; }
}

/// <summary>Governance policy DTO.</summary>
public sealed class GovernancePolicyResponse
{
    public IReadOnlyList<string> AllowedProviders { get; init; } = [];
    public IReadOnlyList<string> AllowedModels { get; init; } = [];
    public decimal? MaximumGpuCostPerHour { get; init; }
    public int? MaximumRunningPods { get; init; }
    public int? MaximumQueueSize { get; init; }
    public decimal? MaximumDailySpendUsd { get; init; }
    public IReadOnlyList<string> AllowedPlugins { get; init; } = [];
    public IReadOnlyList<string> AllowedMcpServers { get; init; } = [];
    public bool EmptyAllowListMeansAllowAll { get; init; } = true;
}

/// <summary>Update policies request.</summary>
public sealed class UpdatePoliciesRequest
{
    public SecurityPolicyResponse? Security { get; set; }
    public GovernancePolicyResponse? Governance { get; set; }
}

/// <summary>Compliance status response.</summary>
public sealed class ComplianceStatusResponse
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

/// <summary>Session history response.</summary>
public sealed class SessionResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime LastSeenAt { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>Trusted device response.</summary>
public sealed class TrustedDeviceResponse
{
    public Guid Id { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string? LastIpAddress { get; init; }
    public DateTime TrustedAt { get; init; }
    public DateTime LastSeenAt { get; init; }
    public bool IsRevoked { get; init; }
}

/// <summary>Security dashboard response.</summary>
public sealed class SecurityDashboardResponse
{
    public int SecurityScore { get; init; }
    public int ActiveSessions { get; init; }
    public int FailedLogins24h { get; init; }
    public int RecentAuditEvents { get; init; }
    public int SecretCount { get; init; }
    public int ExpiringSecrets { get; init; }
    public double MfaCoveragePercent { get; init; }
    public string ComplianceStatus { get; init; } = "Unknown";
    public IReadOnlyList<AuditEventResponse> RecentAudits { get; init; } = [];
}
