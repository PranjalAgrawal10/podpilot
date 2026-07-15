using System.Text.RegularExpressions;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common;

/// <summary>
/// Shared application constants and helpers.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// The ASP.NET Identity Admin role name.
    /// </summary>
    public const string AdminRole = "Admin";

    /// <summary>
    /// The ASP.NET Identity Member role name.
    /// </summary>
    public const string MemberRole = "Member";

    /// <summary>
    /// JWT claim for current organization identifier.
    /// </summary>
    public const string OrganizationIdClaim = "organization_id";

    /// <summary>
    /// JWT claim for current organization role.
    /// </summary>
    public const string OrganizationRoleClaim = "organization_role";

    /// <summary>
    /// Default invitation validity in days.
    /// </summary>
    public const int InvitationExpirationDays = 7;

    /// <summary>
    /// Maximum provider name length.
    /// </summary>
    public const int ProviderNameMaxLength = 200;

    /// <summary>
    /// Maximum provider display name length.
    /// </summary>
    public const int ProviderDisplayNameMaxLength = 200;

    /// <summary>
    /// Maximum provider description length.
    /// </summary>
    public const int ProviderDescriptionMaxLength = 1000;

    /// <summary>
    /// Maximum provider default region length.
    /// </summary>
    public const int ProviderRegionMaxLength = 100;

    /// <summary>
    /// Maximum AI provider base URL length.
    /// </summary>
    public const int AiProviderBaseUrlMaxLength = 500;

    /// <summary>
    /// Maximum AI provider deployment name length.
    /// </summary>
    public const int AiProviderDeploymentNameMaxLength = 200;

    /// <summary>
    /// Maximum AI provider API version length.
    /// </summary>
    public const int AiProviderApiVersionMaxLength = 50;

    /// <summary>
    /// Maximum AI model name length.
    /// </summary>
    public const int AiProviderModelNameMaxLength = 200;

    /// <summary>
    /// Maximum pod name length.
    /// </summary>
    public const int PodNameMaxLength = 191;

    /// <summary>
    /// Maximum pod description length.
    /// </summary>
    public const int PodDescriptionMaxLength = 1000;

    /// <summary>
    /// Maximum pod image name length.
    /// </summary>
    public const int PodImageNameMaxLength = 500;

    /// <summary>
    /// Maximum pod region length.
    /// </summary>
    public const int PodRegionMaxLength = 100;

    /// <summary>
    /// Maximum provider pod identifier length.
    /// </summary>
    public const int ProviderPodIdMaxLength = 100;

    /// <summary>
    /// Minimum container disk size in gigabytes.
    /// </summary>
    public const int PodMinContainerDiskGb = 10;

    /// <summary>
    /// Maximum container disk size in gigabytes.
    /// </summary>
    public const int PodMaxContainerDiskGb = 500;

    /// <summary>
    /// Minimum volume disk size in gigabytes.
    /// </summary>
    public const int PodMinVolumeDiskGb = 0;

    /// <summary>
    /// Maximum volume disk size in gigabytes.
    /// </summary>
    public const int PodMaxVolumeDiskGb = 2000;

    /// <summary>
    /// Default idle timeout in minutes before a pod is considered idle.
    /// </summary>
    public const int DefaultIdleTimeoutMinutes = 30;

    /// <summary>
    /// Default grace period in minutes after idle detection before shutdown.
    /// </summary>
    public const int DefaultGracePeriodMinutes = 5;

    /// <summary>
    /// Default minimum running time in minutes before auto shutdown.
    /// </summary>
    public const int DefaultMinimumRunningTimeMinutes = 10;

    /// <summary>
    /// Default lifecycle lock duration in seconds.
    /// </summary>
    public const int LifecycleLockDurationSeconds = 120;

    /// <summary>
    /// Default Ollama API port on GPU pods.
    /// </summary>
    public const int OllamaPort = 11434;

    /// <summary>
    /// Re-sync pod status from the provider when data is older than this threshold.
    /// </summary>
    public const int PodStatusStaleThresholdSeconds = 60;

    /// <summary>
    /// Maximum wake wait attempts while polling provider health.
    /// </summary>
    public const int MaxWakeHealthCheckAttempts = 30;

    /// <summary>
    /// Gateway API key prefix length.
    /// </summary>
    public const int GatewayApiKeyPrefixLength = 12;

    /// <summary>
    /// Gateway API key total length.
    /// </summary>
    public const int GatewayApiKeyLength = 48;

    /// <summary>
    /// Default gateway requests per minute.
    /// </summary>
    public const int DefaultGatewayRateLimitPerMinute = 60;

    /// <summary>
    /// Default gateway requests per day.
    /// </summary>
    public const int DefaultGatewayRateLimitPerDay = 10000;

    /// <summary>
    /// Maximum Ollama health check attempts during gateway requests.
    /// </summary>
    public const int MaxOllamaHealthCheckAttempts = 60;

    /// <summary>
    /// Maximum Ollama health check attempts when the pod is already running.
    /// </summary>
    public const int MaxOllamaQuickHealthCheckAttempts = 1;

    /// <summary>
    /// Maximum scheduler queue length per organization.
    /// </summary>
    public const int SchedulerMaxQueueLength = 1000;

    /// <summary>
    /// Maximum concurrent requests per GPU pod.
    /// </summary>
    public const int SchedulerMaxConcurrentPerPod = 4;

    /// <summary>
    /// Maximum request body size in bytes for gateway requests.
    /// </summary>
    public const int SchedulerMaxRequestBodyBytes = 10 * 1024 * 1024;

    /// <summary>
    /// Maximum retry attempts for scheduler requests.
    /// </summary>
    public const int SchedulerMaxRetryAttempts = 3;

    /// <summary>
    /// Delay between wake health check attempts.
    /// </summary>
    public static readonly TimeSpan WakeHealthCheckInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Delay between Ollama health check attempts.
    /// </summary>
    public static readonly TimeSpan OllamaHealthCheckInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Per-request timeout for quick Ollama health checks on already-running pods.
    /// </summary>
    public static readonly TimeSpan OllamaQuickHealthCheckTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Default per-request timeout for Ollama health checks.
    /// </summary>
    public static readonly TimeSpan OllamaHealthCheckTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Default gateway forward timeout.
    /// </summary>
    public static readonly TimeSpan GatewayForwardTimeout = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Base retry delay for exponential backoff.
    /// </summary>
    public static readonly TimeSpan SchedulerRetryBaseDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Maximum time a request may wait in the queue.
    /// </summary>
    public static readonly TimeSpan SchedulerQueueTimeout = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Distributed lock expiry for scheduler operations.
    /// </summary>
    public static readonly TimeSpan SchedulerLockExpiry = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Scheduler worker polling interval.
    /// </summary>
    public static readonly TimeSpan SchedulerWorkerInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// All ASP.NET Identity roles.
    /// </summary>
    public static readonly string[] AllRoles = [AdminRole, MemberRole];

    /// <summary>
    /// Creates a URL-friendly slug from an organization name.
    /// </summary>
    /// <param name="name">The organization name.</param>
    /// <returns>A URL-friendly slug.</returns>
    public static string CreateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    /// <summary>
    /// Maps an <see cref="OrganizationRole"/> to its string representation.
    /// </summary>
    /// <param name="role">The role enum value.</param>
    /// <returns>The role name.</returns>
    public static string ToRoleName(OrganizationRole role) => role.ToString();

    /// <summary>
    /// Parses a role name into an <see cref="OrganizationRole"/>.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>The parsed role.</returns>
    public static OrganizationRole ParseRoleName(string roleName) =>
        Enum.TryParse<OrganizationRole>(roleName, ignoreCase: true, out var role)
            ? role
            : OrganizationRole.Viewer;
}
