using System.Text.Json;
using PodPilot.Application.Models.Security;
using PodPilot.Contracts.Security;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Security;

/// <summary>
/// Maps security domain entities to contract responses.
/// </summary>
internal static class SecurityMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Maps an identity provider.</summary>
    public static IdentityProviderResponse ToIdentityProviderResponse(IdentityProvider provider) =>
        new()
        {
            Id = provider.Id,
            Name = provider.Name,
            ProviderKind = provider.ProviderKind.ToString(),
            Protocol = provider.Protocol.ToString(),
            IsEnabled = provider.IsEnabled,
            Issuer = provider.Issuer,
            ClientId = provider.ClientId,
            HasClientSecret = !string.IsNullOrWhiteSpace(provider.EncryptedClientSecret),
            Scopes = provider.Scopes,
            CreatedAt = provider.CreatedAt,
        };

    /// <summary>Maps a secret reference (never includes plaintext).</summary>
    public static SecretResponse ToSecretResponse(SecretReference secret) =>
        new()
        {
            Id = secret.Id,
            Name = secret.Name,
            SecretKind = secret.SecretKind.ToString(),
            BackendKind = secret.BackendKind.ToString(),
            ExpiresAt = secret.ExpiresAt,
            LastRotatedAt = secret.LastRotatedAt,
            LastAccessedAt = secret.LastAccessedAt,
            IsEnabled = secret.IsEnabled,
            Version = secret.Version,
            CreatedAt = secret.CreatedAt,
        };

    /// <summary>Maps an enterprise audit entry.</summary>
    public static AuditEventResponse ToAuditEventResponse(EnterpriseAuditEntry entry) =>
        new()
        {
            Id = entry.Id,
            OrganizationId = entry.OrganizationId,
            UserId = entry.UserId,
            ActorEmail = entry.ActorEmail,
            Category = entry.Category.ToString(),
            EventType = entry.EventType,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Summary = entry.Summary,
            IpAddress = entry.IpAddress,
            OccurredAt = entry.OccurredAt,
        };

    /// <summary>Maps a persisted audit event.</summary>
    public static AuditEventResponse ToAuditEventResponse(AuditEvent entity) =>
        new()
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            UserId = entity.UserId,
            ActorEmail = entity.ActorEmail,
            Category = entity.Category.ToString(),
            EventType = entity.EventType,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            Summary = entity.Summary,
            IpAddress = entity.IpAddress,
            OccurredAt = entity.OccurredAt,
        };

    /// <summary>Maps security and governance policies.</summary>
    public static OrganizationPoliciesResponse ToPoliciesResponse(
        OrganizationSecurityPolicy? security,
        OrganizationGovernancePolicy? governance) =>
        new()
        {
            Security = security is null
                ? new SecurityPolicyResponse()
                : new SecurityPolicyResponse
                {
                    MinPasswordLength = security.MinPasswordLength,
                    RequireUppercase = security.RequireUppercase,
                    RequireDigit = security.RequireDigit,
                    RequireNonAlphanumeric = security.RequireNonAlphanumeric,
                    RequireMfa = security.RequireMfa,
                    SessionTimeoutMinutes = security.SessionTimeoutMinutes,
                    MaxConcurrentSessions = security.MaxConcurrentSessions,
                    IpAllowList = ParseStringList(security.IpAllowListJson),
                    GeoAllowList = ParseStringList(security.GeoAllowListJson),
                    ApiKeyExpirationDays = security.ApiKeyExpirationDays,
                    EnforceApiKeyRotation = security.EnforceApiKeyRotation,
                    FailedLoginAlertThreshold = security.FailedLoginAlertThreshold,
                },
            Governance = governance is null
                ? new GovernancePolicyResponse()
                : new GovernancePolicyResponse
                {
                    AllowedProviders = ParseStringList(governance.AllowedProvidersJson),
                    AllowedModels = ParseStringList(governance.AllowedModelsJson),
                    MaximumGpuCostPerHour = governance.MaximumGpuCostPerHour,
                    MaximumRunningPods = governance.MaximumRunningPods,
                    MaximumQueueSize = governance.MaximumQueueSize,
                    MaximumDailySpendUsd = governance.MaximumDailySpendUsd,
                    AllowedPlugins = ParseStringList(governance.AllowedPluginsJson),
                    AllowedMcpServers = ParseStringList(governance.AllowedMcpServersJson),
                    EmptyAllowListMeansAllowAll = governance.EmptyAllowListMeansAllowAll,
                },
        };

    /// <summary>Maps compliance status.</summary>
    public static ComplianceStatusResponse ToComplianceResponse(ComplianceStatus status) =>
        new()
        {
            GdprEnabled = status.GdprEnabled,
            Soc2Enabled = status.Soc2Enabled,
            Iso27001Enabled = status.Iso27001Enabled,
            DataRetentionDays = status.DataRetentionDays,
            LogRetentionDays = status.LogRetentionDays,
            LastExportAt = status.LastExportAt,
            LastErasureAt = status.LastErasureAt,
            OverallStatus = status.OverallStatus,
            ControlChecklist = status.ControlChecklist,
        };

    /// <summary>Maps a session info record.</summary>
    public static SessionResponse ToSessionResponse(SessionInfo session) =>
        new()
        {
            Id = session.Id,
            UserId = session.UserId,
            SessionId = session.SessionId,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            StartedAt = session.StartedAt,
            LastSeenAt = session.LastSeenAt,
            IsActive = true,
        };

    /// <summary>Maps a session history entity.</summary>
    public static SessionResponse ToSessionResponse(SessionHistory session) =>
        new()
        {
            Id = session.Id,
            UserId = session.UserId,
            SessionId = session.SessionId,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            StartedAt = session.StartedAt,
            LastSeenAt = session.LastSeenAt,
            IsActive = session.IsActive,
        };

    /// <summary>Maps a trusted device.</summary>
    public static TrustedDeviceResponse ToTrustedDeviceResponse(TrustedDevice device) =>
        new()
        {
            Id = device.Id,
            DeviceName = device.DeviceName,
            LastIpAddress = device.LastIpAddress,
            TrustedAt = device.TrustedAt,
            LastSeenAt = device.LastSeenAt,
            IsRevoked = device.IsRevoked,
        };

    /// <summary>Maps a security dashboard.</summary>
    public static SecurityDashboardResponse ToDashboardResponse(SecurityDashboard dashboard) =>
        new()
        {
            SecurityScore = dashboard.SecurityScore,
            ActiveSessions = dashboard.ActiveSessions,
            FailedLogins24h = dashboard.FailedLogins24h,
            RecentAuditEvents = dashboard.RecentAuditEvents,
            SecretCount = dashboard.SecretCount,
            ExpiringSecrets = dashboard.ExpiringSecrets,
            MfaCoveragePercent = dashboard.MfaCoveragePercent,
            ComplianceStatus = dashboard.ComplianceStatus,
            RecentAudits = dashboard.RecentAudits.Select(ToAuditEventResponse).ToList(),
        };

    /// <summary>Maps MFA enrollment.</summary>
    public static MfaEnrollmentResponse ToMfaEnrollmentResponse(MfaEnrollmentResult result) =>
        new()
        {
            SharedSecret = result.SharedSecret,
            OtpAuthUri = result.OtpAuthUri,
        };

    /// <summary>Maps SSO challenge.</summary>
    public static SsoChallengeResponse ToSsoChallengeResponse(SsoChallengeResult result) =>
        new()
        {
            AuthorizationUrl = result.AuthorizationUrl,
            State = result.State,
        };

    /// <summary>Serializes a string list to JSON.</summary>
    public static string ToJsonArray(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize(values ?? [], JsonOptions);

    /// <summary>Parses a JSON string array.</summary>
    public static IReadOnlyList<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
