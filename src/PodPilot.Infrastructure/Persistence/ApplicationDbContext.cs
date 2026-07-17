using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Identity;

namespace PodPilot.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <inheritdoc />
    public DbSet<Organization> Organizations => Set<Organization>();

    /// <inheritdoc />
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    /// <inheritdoc />
    public DbSet<Invitation> Invitations => Set<Invitation>();

    /// <inheritdoc />
    public DbSet<Permission> Permissions => Set<Permission>();

    /// <inheritdoc />
    public DbSet<Role> OrgRoles => Set<Role>();

    /// <inheritdoc />
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    public DbSet<ComputeProvider> ComputeProviders => Set<ComputeProvider>();

    /// <inheritdoc />
    public DbSet<ProviderCredential> ProviderCredentials => Set<ProviderCredential>();

    /// <inheritdoc />
    public DbSet<ProviderRegion> ProviderRegions => Set<ProviderRegion>();

    /// <inheritdoc />
    public DbSet<ProviderGpu> ProviderGpus => Set<ProviderGpu>();

    /// <inheritdoc />
    public DbSet<ProviderHealth> ProviderHealthSnapshots => Set<ProviderHealth>();

    /// <inheritdoc />
    public DbSet<ProviderHealthHistory> ProviderHealthHistoryEntries => Set<ProviderHealthHistory>();

    /// <inheritdoc />
    public DbSet<GpuPod> GpuPods => Set<GpuPod>();

    /// <inheritdoc />
    public DbSet<PodConfiguration> PodConfigurations => Set<PodConfiguration>();

    /// <inheritdoc />
    public DbSet<PodEndpoint> PodEndpoints => Set<PodEndpoint>();

    /// <inheritdoc />
    public DbSet<PodStatusHistory> PodStatusHistoryEntries => Set<PodStatusHistory>();

    /// <inheritdoc />
    public DbSet<PodActivity> PodActivities => Set<PodActivity>();

    /// <inheritdoc />
    public DbSet<PodLifecycleEvent> PodLifecycleEvents => Set<PodLifecycleEvent>();

    /// <inheritdoc />
    public DbSet<PodIdlePolicy> PodIdlePolicies => Set<PodIdlePolicy>();

    /// <inheritdoc />
    public DbSet<PodLifecycleLock> PodLifecycleLocks => Set<PodLifecycleLock>();

    /// <inheritdoc />
    public DbSet<PodWakeRequest> PodWakeRequests => Set<PodWakeRequest>();

    /// <inheritdoc />
    public DbSet<GatewayApiKey> GatewayApiKeys => Set<GatewayApiKey>();

    /// <inheritdoc />
    public DbSet<GatewayRoute> GatewayRoutes => Set<GatewayRoute>();

    /// <inheritdoc />
    public DbSet<GatewayRequest> GatewayRequests => Set<GatewayRequest>();

    /// <inheritdoc />
    public DbSet<RequestQueueEntry> RequestQueueEntries => Set<RequestQueueEntry>();

    /// <inheritdoc />
    public DbSet<RequestExecution> RequestExecutions => Set<RequestExecution>();

    /// <inheritdoc />
    public DbSet<SchedulerEvent> SchedulerEvents => Set<SchedulerEvent>();

    /// <inheritdoc />
    public DbSet<AiModel> AiModels => Set<AiModel>();

    /// <inheritdoc />
    public DbSet<ModelDownload> ModelDownloads => Set<ModelDownload>();

    /// <inheritdoc />
    public DbSet<ModelHealthHistory> ModelHealthHistoryEntries => Set<ModelHealthHistory>();

    /// <inheritdoc />
    public DbSet<DatabaseMigrationHistory> DatabaseMigrationHistoryEntries => Set<DatabaseMigrationHistory>();

    /// <inheritdoc />
    public DbSet<DatabaseSeedHistory> DatabaseSeedHistoryEntries => Set<DatabaseSeedHistory>();

    /// <inheritdoc />
    public DbSet<PodPool> PodPools => Set<PodPool>();

    /// <inheritdoc />
    public DbSet<PodPoolMember> PodPoolMembers => Set<PodPoolMember>();

    /// <inheritdoc />
    public DbSet<PodPoolModel> PodPoolModels => Set<PodPoolModel>();

    /// <inheritdoc />
    public DbSet<ScalingPolicy> ScalingPolicies => Set<ScalingPolicy>();

    /// <inheritdoc />
    public DbSet<ScalingEvent> ScalingEvents => Set<ScalingEvent>();

    /// <inheritdoc />
    public DbSet<PodHealthMetric> PodHealthMetrics => Set<PodHealthMetric>();

    /// <inheritdoc />
    public DbSet<CapacitySnapshot> CapacitySnapshots => Set<CapacitySnapshot>();

    /// <inheritdoc />
    public DbSet<LoadBalancerConfig> LoadBalancerConfigs => Set<LoadBalancerConfig>();

    /// <inheritdoc />
    public DbSet<MetricsSnapshot> MetricsSnapshots => Set<MetricsSnapshot>();

    /// <inheritdoc />
    public DbSet<CostSnapshot> CostSnapshots => Set<CostSnapshot>();

    /// <inheritdoc />
    public DbSet<UsageStatistics> UsageStatisticsEntries => Set<UsageStatistics>();

    /// <inheritdoc />
    public DbSet<AlertHistory> AlertHistoryEntries => Set<AlertHistory>();

    /// <inheritdoc />
    public DbSet<SystemHealthHistory> SystemHealthHistoryEntries => Set<SystemHealthHistory>();

    /// <inheritdoc />
    public DbSet<AiInferenceProvider> AiInferenceProviders => Set<AiInferenceProvider>();

    /// <inheritdoc />
    public DbSet<AiProviderCredential> AiProviderCredentials => Set<AiProviderCredential>();

    /// <inheritdoc />
    public DbSet<AiProviderModel> AiProviderModels => Set<AiProviderModel>();

    /// <inheritdoc />
    public DbSet<AiProviderHealth> AiProviderHealthSnapshots => Set<AiProviderHealth>();

    /// <inheritdoc />
    public DbSet<AiRoutingPolicy> AiRoutingPolicies => Set<AiRoutingPolicy>();

    /// <inheritdoc />
    public DbSet<AiFailoverEvent> AiFailoverEvents => Set<AiFailoverEvent>();

    /// <inheritdoc />
    public DbSet<ModelScore> ModelScores => Set<ModelScore>();

    /// <inheritdoc />
    public DbSet<LatencyHistory> LatencyHistories => Set<LatencyHistory>();

    /// <inheritdoc />
    public DbSet<CostHistory> CostHistories => Set<CostHistory>();

    /// <inheritdoc />
    public DbSet<RoutingEvent> RoutingEvents => Set<RoutingEvent>();

    /// <inheritdoc />
    public DbSet<PluginDefinition> PluginDefinitions => Set<PluginDefinition>();

    /// <inheritdoc />
    public DbSet<PluginInstallation> PluginInstallations => Set<PluginInstallation>();

    /// <inheritdoc />
    public DbSet<PluginSetting> PluginSettings => Set<PluginSetting>();

    /// <inheritdoc />
    public DbSet<PluginLog> PluginLogs => Set<PluginLog>();

    /// <inheritdoc />
    public DbSet<McpServer> McpServers => Set<McpServer>();

    /// <inheritdoc />
    public DbSet<McpTool> McpTools => Set<McpTool>();

    /// <inheritdoc />
    public DbSet<McpResource> McpResources => Set<McpResource>();

    /// <inheritdoc />
    public DbSet<McpPrompt> McpPrompts => Set<McpPrompt>();

    /// <inheritdoc />
    public DbSet<McpToolExecution> McpToolExecutions => Set<McpToolExecution>();

    /// <inheritdoc />
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    /// <inheritdoc />
    public DbSet<IdentityProvider> IdentityProviders => Set<IdentityProvider>();

    /// <inheritdoc />
    public DbSet<ScimMapping> ScimMappings => Set<ScimMapping>();

    /// <inheritdoc />
    public DbSet<SecretReference> SecretReferences => Set<SecretReference>();

    /// <inheritdoc />
    public DbSet<ComplianceEvent> ComplianceEvents => Set<ComplianceEvent>();

    /// <inheritdoc />
    public DbSet<SessionHistory> SessionHistories => Set<SessionHistory>();

    /// <inheritdoc />
    public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();

    /// <inheritdoc />
    public DbSet<OrganizationSecurityPolicy> OrganizationSecurityPolicies => Set<OrganizationSecurityPolicy>();

    /// <inheritdoc />
    public DbSet<OrganizationGovernancePolicy> OrganizationGovernancePolicies => Set<OrganizationGovernancePolicy>();

    /// <inheritdoc />
    public DbSet<OrganizationComplianceSettings> OrganizationComplianceSettings => Set<OrganizationComplianceSettings>();

    /// <inheritdoc />
    public DbSet<UserMfaEnrollment> UserMfaEnrollments => Set<UserMfaEnrollment>();

    /// <inheritdoc />
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    /// <inheritdoc />
    public DbSet<PlanQuota> PlanQuotas => Set<PlanQuota>();

    /// <inheritdoc />
    public DbSet<OrganizationSubscription> OrganizationSubscriptions => Set<OrganizationSubscription>();

    /// <inheritdoc />
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    /// <inheritdoc />
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <inheritdoc />
    public DbSet<ProductLicense> ProductLicenses => Set<ProductLicense>();

    /// <inheritdoc />
    public DbSet<OnboardingProgress> OnboardingProgressRecords => Set<OnboardingProgress>();

    /// <inheritdoc />
    public DbSet<TelemetryPreference> TelemetryPreferences => Set<TelemetryPreference>();

    /// <inheritdoc />
    public DbSet<BackupJob> BackupJobs => Set<BackupJob>();

    /// <inheritdoc />
    public DbSet<PlatformRelease> PlatformReleases => Set<PlatformRelease>();

    /// <inheritdoc />
    public DbSet<AiDeployment> AiDeployments => Set<AiDeployment>();

    /// <inheritdoc />
    public DbSet<DeploymentTemplate> DeploymentTemplates => Set<DeploymentTemplate>();

    /// <inheritdoc />
    public DbSet<DeploymentModel> DeploymentModels => Set<DeploymentModel>();

    /// <inheritdoc />
    public DbSet<DeploymentLog> DeploymentLogs => Set<DeploymentLog>();

    /// <inheritdoc />
    public DbSet<RuntimeVersion> RuntimeVersions => Set<RuntimeVersion>();

    /// <inheritdoc />
    public DbSet<GpuCatalogEntry> GpuCatalogEntries => Set<GpuCatalogEntry>();

    /// <inheritdoc />
    public DbSet<ModelCatalogEntry> ModelCatalogEntries => Set<ModelCatalogEntry>();

    /// <inheritdoc />
    public DbSet<DeploymentHealth> DeploymentHealthSnapshots => Set<DeploymentHealth>();

    /// <inheritdoc />
    public DbSet<DeploymentHistory> DeploymentHistoryEntries => Set<DeploymentHistory>();

    IQueryable<RefreshToken> IApplicationDbContext.RefreshTokens => RefreshTokens;

    IQueryable<Organization> IApplicationDbContext.Organizations => Organizations;

    IQueryable<OrganizationMember> IApplicationDbContext.OrganizationMembers => OrganizationMembers;

    IQueryable<Invitation> IApplicationDbContext.Invitations => Invitations;

    IQueryable<Permission> IApplicationDbContext.Permissions => Permissions;

    IQueryable<Role> IApplicationDbContext.Roles => OrgRoles;

    IQueryable<AuditLog> IApplicationDbContext.AuditLogs => AuditLogs;

    IQueryable<ComputeProvider> IApplicationDbContext.ComputeProviders => ComputeProviders;

    IQueryable<ProviderCredential> IApplicationDbContext.ProviderCredentials => ProviderCredentials;

    IQueryable<ProviderRegion> IApplicationDbContext.ProviderRegions => ProviderRegions;

    IQueryable<ProviderGpu> IApplicationDbContext.ProviderGpus => ProviderGpus;

    IQueryable<ProviderHealth> IApplicationDbContext.ProviderHealthSnapshots => ProviderHealthSnapshots;

    IQueryable<ProviderHealthHistory> IApplicationDbContext.ProviderHealthHistory => ProviderHealthHistoryEntries;

    IQueryable<GpuPod> IApplicationDbContext.GpuPods => GpuPods;

    IQueryable<PodConfiguration> IApplicationDbContext.PodConfigurations => PodConfigurations;

    IQueryable<PodEndpoint> IApplicationDbContext.PodEndpoints => PodEndpoints;

    IQueryable<PodStatusHistory> IApplicationDbContext.PodStatusHistory => PodStatusHistoryEntries;

    IQueryable<PodActivity> IApplicationDbContext.PodActivities => PodActivities;

    IQueryable<PodLifecycleEvent> IApplicationDbContext.PodLifecycleEvents => PodLifecycleEvents;

    IQueryable<PodIdlePolicy> IApplicationDbContext.PodIdlePolicies => PodIdlePolicies;

    IQueryable<PodLifecycleLock> IApplicationDbContext.PodLifecycleLocks => PodLifecycleLocks;

    IQueryable<PodWakeRequest> IApplicationDbContext.PodWakeRequests => PodWakeRequests;

    IQueryable<GatewayApiKey> IApplicationDbContext.GatewayApiKeys => GatewayApiKeys;

    IQueryable<GatewayRoute> IApplicationDbContext.GatewayRoutes => GatewayRoutes;

    IQueryable<GatewayRequest> IApplicationDbContext.GatewayRequests => GatewayRequests;

    IQueryable<RequestQueueEntry> IApplicationDbContext.RequestQueueEntries => RequestQueueEntries;

    IQueryable<RequestExecution> IApplicationDbContext.RequestExecutions => RequestExecutions;

    IQueryable<SchedulerEvent> IApplicationDbContext.SchedulerEvents => SchedulerEvents;

    IQueryable<AiModel> IApplicationDbContext.AiModels => AiModels;

    IQueryable<ModelDownload> IApplicationDbContext.ModelDownloads => ModelDownloads;

    IQueryable<ModelHealthHistory> IApplicationDbContext.ModelHealthHistory => ModelHealthHistoryEntries;

    IQueryable<DatabaseMigrationHistory> IApplicationDbContext.DatabaseMigrationHistory => DatabaseMigrationHistoryEntries;

    IQueryable<DatabaseSeedHistory> IApplicationDbContext.DatabaseSeedHistory => DatabaseSeedHistoryEntries;

    IQueryable<PodPool> IApplicationDbContext.PodPools => PodPools;

    IQueryable<PodPoolMember> IApplicationDbContext.PodPoolMembers => PodPoolMembers;

    IQueryable<PodPoolModel> IApplicationDbContext.PodPoolModels => PodPoolModels;

    IQueryable<ScalingPolicy> IApplicationDbContext.ScalingPolicies => ScalingPolicies;

    IQueryable<ScalingEvent> IApplicationDbContext.ScalingEvents => ScalingEvents;

    IQueryable<PodHealthMetric> IApplicationDbContext.PodHealthMetrics => PodHealthMetrics;

    IQueryable<CapacitySnapshot> IApplicationDbContext.CapacitySnapshots => CapacitySnapshots;

    IQueryable<LoadBalancerConfig> IApplicationDbContext.LoadBalancerConfigs => LoadBalancerConfigs;

    IQueryable<MetricsSnapshot> IApplicationDbContext.MetricsSnapshots => MetricsSnapshots;

    IQueryable<CostSnapshot> IApplicationDbContext.CostSnapshots => CostSnapshots;

    IQueryable<UsageStatistics> IApplicationDbContext.UsageStatistics => UsageStatisticsEntries;

    IQueryable<AlertHistory> IApplicationDbContext.AlertHistory => AlertHistoryEntries;

    IQueryable<SystemHealthHistory> IApplicationDbContext.SystemHealthHistory => SystemHealthHistoryEntries;

    IQueryable<AiInferenceProvider> IApplicationDbContext.AiInferenceProviders => AiInferenceProviders;

    IQueryable<AiProviderCredential> IApplicationDbContext.AiProviderCredentials => AiProviderCredentials;

    IQueryable<AiProviderModel> IApplicationDbContext.AiProviderModels => AiProviderModels;

    IQueryable<AiProviderHealth> IApplicationDbContext.AiProviderHealthSnapshots => AiProviderHealthSnapshots;

    IQueryable<AiRoutingPolicy> IApplicationDbContext.AiRoutingPolicies => AiRoutingPolicies;

    IQueryable<AiFailoverEvent> IApplicationDbContext.AiFailoverEvents => AiFailoverEvents;

    IQueryable<ModelScore> IApplicationDbContext.ModelScores => ModelScores;

    IQueryable<LatencyHistory> IApplicationDbContext.LatencyHistories => LatencyHistories;

    IQueryable<CostHistory> IApplicationDbContext.CostHistories => CostHistories;

    IQueryable<RoutingEvent> IApplicationDbContext.RoutingEvents => RoutingEvents;

    IQueryable<PluginDefinition> IApplicationDbContext.PluginDefinitions => PluginDefinitions;

    IQueryable<PluginInstallation> IApplicationDbContext.PluginInstallations => PluginInstallations;

    IQueryable<PluginSetting> IApplicationDbContext.PluginSettings => PluginSettings;

    IQueryable<PluginLog> IApplicationDbContext.PluginLogs => PluginLogs;

    IQueryable<McpServer> IApplicationDbContext.McpServers => McpServers;

    IQueryable<McpTool> IApplicationDbContext.McpTools => McpTools;

    IQueryable<McpResource> IApplicationDbContext.McpResources => McpResources;

    IQueryable<McpPrompt> IApplicationDbContext.McpPrompts => McpPrompts;

    IQueryable<McpToolExecution> IApplicationDbContext.McpToolExecutions => McpToolExecutions;

    IQueryable<AuditEvent> IApplicationDbContext.AuditEvents => AuditEvents;

    IQueryable<IdentityProvider> IApplicationDbContext.IdentityProviders => IdentityProviders;

    IQueryable<ScimMapping> IApplicationDbContext.ScimMappings => ScimMappings;

    IQueryable<SecretReference> IApplicationDbContext.SecretReferences => SecretReferences;

    IQueryable<ComplianceEvent> IApplicationDbContext.ComplianceEvents => ComplianceEvents;

    IQueryable<SessionHistory> IApplicationDbContext.SessionHistories => SessionHistories;

    IQueryable<TrustedDevice> IApplicationDbContext.TrustedDevices => TrustedDevices;

    IQueryable<OrganizationSecurityPolicy> IApplicationDbContext.OrganizationSecurityPolicies => OrganizationSecurityPolicies;

    IQueryable<OrganizationGovernancePolicy> IApplicationDbContext.OrganizationGovernancePolicies => OrganizationGovernancePolicies;

    IQueryable<OrganizationComplianceSettings> IApplicationDbContext.OrganizationComplianceSettings => OrganizationComplianceSettings;

    IQueryable<UserMfaEnrollment> IApplicationDbContext.UserMfaEnrollments => UserMfaEnrollments;

    IQueryable<SubscriptionPlan> IApplicationDbContext.SubscriptionPlans => SubscriptionPlans;

    IQueryable<PlanQuota> IApplicationDbContext.PlanQuotas => PlanQuotas;

    IQueryable<OrganizationSubscription> IApplicationDbContext.OrganizationSubscriptions => OrganizationSubscriptions;

    IQueryable<UsageRecord> IApplicationDbContext.UsageRecords => UsageRecords;

    IQueryable<Invoice> IApplicationDbContext.Invoices => Invoices;

    IQueryable<ProductLicense> IApplicationDbContext.ProductLicenses => ProductLicenses;

    IQueryable<OnboardingProgress> IApplicationDbContext.OnboardingProgressRecords => OnboardingProgressRecords;

    IQueryable<TelemetryPreference> IApplicationDbContext.TelemetryPreferences => TelemetryPreferences;

    IQueryable<BackupJob> IApplicationDbContext.BackupJobs => BackupJobs;

    IQueryable<PlatformRelease> IApplicationDbContext.PlatformReleases => PlatformReleases;

    IQueryable<AiDeployment> IApplicationDbContext.AiDeployments => AiDeployments;

    IQueryable<DeploymentTemplate> IApplicationDbContext.DeploymentTemplates => DeploymentTemplates;

    IQueryable<DeploymentModel> IApplicationDbContext.DeploymentModels => DeploymentModels;

    IQueryable<DeploymentLog> IApplicationDbContext.DeploymentLogs => DeploymentLogs;

    IQueryable<RuntimeVersion> IApplicationDbContext.RuntimeVersions => RuntimeVersions;

    IQueryable<GpuCatalogEntry> IApplicationDbContext.GpuCatalogEntries => GpuCatalogEntries;

    IQueryable<ModelCatalogEntry> IApplicationDbContext.ModelCatalogEntries => ModelCatalogEntries;

    IQueryable<DeploymentHealth> IApplicationDbContext.DeploymentHealthSnapshots => DeploymentHealthSnapshots;

    IQueryable<DeploymentHistory> IApplicationDbContext.DeploymentHistoryEntries => DeploymentHistoryEntries;

    /// <inheritdoc />
    public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) =>
        AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default) =>
        Organizations.AddAsync(organization, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default) =>
        OrganizationMembers.AddAsync(member, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddInvitationAsync(Invitation invitation, CancellationToken cancellationToken = default) =>
        Invitations.AddAsync(invitation, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddComputeProviderAsync(ComputeProvider provider, CancellationToken cancellationToken = default) =>
        ComputeProviders.AddAsync(provider, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderCredentialAsync(ProviderCredential credential, CancellationToken cancellationToken = default) =>
        ProviderCredentials.AddAsync(credential, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderHealthAsync(ProviderHealth health, CancellationToken cancellationToken = default) =>
        ProviderHealthSnapshots.AddAsync(health, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderHealthHistoryAsync(ProviderHealthHistory history, CancellationToken cancellationToken = default) =>
        ProviderHealthHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderRegionAsync(ProviderRegion region, CancellationToken cancellationToken = default) =>
        ProviderRegions.AddAsync(region, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderGpuAsync(ProviderGpu gpu, CancellationToken cancellationToken = default) =>
        ProviderGpus.AddAsync(gpu, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task ClearProviderCatalogAsync(Guid computeProviderId, CancellationToken cancellationToken = default)
    {
        var regions = await ProviderRegions
            .Where(r => r.ComputeProviderId == computeProviderId)
            .ToListAsync(cancellationToken);

        if (regions.Count > 0)
        {
            ProviderRegions.RemoveRange(regions);
        }

        var gpus = await ProviderGpus
            .Where(g => g.ComputeProviderId == computeProviderId)
            .ToListAsync(cancellationToken);

        if (gpus.Count > 0)
        {
            ProviderGpus.RemoveRange(gpus);
        }
    }

    /// <inheritdoc />
    public async Task RemoveComputeProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var provider = await ComputeProviders
            .FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);

        if (provider is not null)
        {
            ComputeProviders.Remove(provider);
        }
    }

    /// <inheritdoc />
    public Task AddGpuPodAsync(GpuPod pod, CancellationToken cancellationToken = default) =>
        GpuPods.AddAsync(pod, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodStatusHistoryAsync(PodStatusHistory history, CancellationToken cancellationToken = default) =>
        PodStatusHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodActivityAsync(PodActivity activity, CancellationToken cancellationToken = default) =>
        PodActivities.AddAsync(activity, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodLifecycleEventAsync(PodLifecycleEvent lifecycleEvent, CancellationToken cancellationToken = default) =>
        PodLifecycleEvents.AddAsync(lifecycleEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodIdlePolicyAsync(PodIdlePolicy policy, CancellationToken cancellationToken = default) =>
        PodIdlePolicies.AddAsync(policy, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodWakeRequestAsync(PodWakeRequest request, CancellationToken cancellationToken = default) =>
        PodWakeRequests.AddAsync(request, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default) =>
        PodLifecycleLocks.AddAsync(lifecycleLock, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGatewayApiKeyAsync(GatewayApiKey apiKey, CancellationToken cancellationToken = default) =>
        GatewayApiKeys.AddAsync(apiKey, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGatewayRouteAsync(GatewayRoute route, CancellationToken cancellationToken = default) =>
        GatewayRoutes.AddAsync(route, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGatewayRequestAsync(GatewayRequest request, CancellationToken cancellationToken = default) =>
        GatewayRequests.AddAsync(request, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddRequestQueueEntryAsync(RequestQueueEntry entry, CancellationToken cancellationToken = default) =>
        RequestQueueEntries.AddAsync(entry, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddRequestExecutionAsync(RequestExecution execution, CancellationToken cancellationToken = default) =>
        RequestExecutions.AddAsync(execution, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddSchedulerEventAsync(SchedulerEvent schedulerEvent, CancellationToken cancellationToken = default) =>
        SchedulerEvents.AddAsync(schedulerEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiModelAsync(AiModel model, CancellationToken cancellationToken = default) =>
        AiModels.AddAsync(model, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddModelDownloadAsync(ModelDownload download, CancellationToken cancellationToken = default) =>
        ModelDownloads.AddAsync(download, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddModelHealthHistoryAsync(ModelHealthHistory history, CancellationToken cancellationToken = default) =>
        ModelHealthHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodPoolAsync(PodPool pool, CancellationToken cancellationToken = default) =>
        PodPools.AddAsync(pool, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodPoolMemberAsync(PodPoolMember member, CancellationToken cancellationToken = default) =>
        PodPoolMembers.AddAsync(member, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodPoolModelAsync(PodPoolModel model, CancellationToken cancellationToken = default) =>
        PodPoolModels.AddAsync(model, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddScalingPolicyAsync(ScalingPolicy policy, CancellationToken cancellationToken = default) =>
        ScalingPolicies.AddAsync(policy, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddScalingEventAsync(ScalingEvent scalingEvent, CancellationToken cancellationToken = default) =>
        ScalingEvents.AddAsync(scalingEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodHealthMetricAsync(PodHealthMetric metric, CancellationToken cancellationToken = default) =>
        PodHealthMetrics.AddAsync(metric, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddCapacitySnapshotAsync(CapacitySnapshot snapshot, CancellationToken cancellationToken = default) =>
        CapacitySnapshots.AddAsync(snapshot, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddLoadBalancerConfigAsync(LoadBalancerConfig config, CancellationToken cancellationToken = default) =>
        LoadBalancerConfigs.AddAsync(config, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddMetricsSnapshotAsync(MetricsSnapshot snapshot, CancellationToken cancellationToken = default) =>
        MetricsSnapshots.AddAsync(snapshot, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddCostSnapshotAsync(CostSnapshot snapshot, CancellationToken cancellationToken = default) =>
        CostSnapshots.AddAsync(snapshot, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddUsageStatisticsAsync(UsageStatistics statistics, CancellationToken cancellationToken = default) =>
        UsageStatisticsEntries.AddAsync(statistics, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAlertHistoryAsync(AlertHistory alert, CancellationToken cancellationToken = default) =>
        AlertHistoryEntries.AddAsync(alert, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddSystemHealthHistoryAsync(SystemHealthHistory history, CancellationToken cancellationToken = default) =>
        SystemHealthHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiInferenceProviderAsync(AiInferenceProvider provider, CancellationToken cancellationToken = default) =>
        AiInferenceProviders.AddAsync(provider, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiProviderCredentialAsync(AiProviderCredential credential, CancellationToken cancellationToken = default) =>
        AiProviderCredentials.AddAsync(credential, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiProviderModelAsync(AiProviderModel model, CancellationToken cancellationToken = default) =>
        AiProviderModels.AddAsync(model, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiProviderHealthAsync(AiProviderHealth health, CancellationToken cancellationToken = default) =>
        AiProviderHealthSnapshots.AddAsync(health, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiRoutingPolicyAsync(AiRoutingPolicy policy, CancellationToken cancellationToken = default) =>
        AiRoutingPolicies.AddAsync(policy, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiFailoverEventAsync(AiFailoverEvent failoverEvent, CancellationToken cancellationToken = default) =>
        AiFailoverEvents.AddAsync(failoverEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddModelScoreAsync(ModelScore score, CancellationToken cancellationToken = default) =>
        ModelScores.AddAsync(score, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddLatencyHistoryAsync(LatencyHistory history, CancellationToken cancellationToken = default) =>
        LatencyHistories.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddCostHistoryAsync(CostHistory history, CancellationToken cancellationToken = default) =>
        CostHistories.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddRoutingEventAsync(RoutingEvent routingEvent, CancellationToken cancellationToken = default) =>
        RoutingEvents.AddAsync(routingEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPluginDefinitionAsync(PluginDefinition definition, CancellationToken cancellationToken = default) =>
        PluginDefinitions.AddAsync(definition, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPluginInstallationAsync(PluginInstallation installation, CancellationToken cancellationToken = default) =>
        PluginInstallations.AddAsync(installation, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPluginSettingAsync(PluginSetting setting, CancellationToken cancellationToken = default) =>
        PluginSettings.AddAsync(setting, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPluginLogAsync(PluginLog log, CancellationToken cancellationToken = default) =>
        PluginLogs.AddAsync(log, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddMcpServerAsync(McpServer server, CancellationToken cancellationToken = default) =>
        McpServers.AddAsync(server, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddMcpToolAsync(McpTool tool, CancellationToken cancellationToken = default) =>
        McpTools.AddAsync(tool, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddMcpResourceAsync(McpResource resource, CancellationToken cancellationToken = default) =>
        McpResources.AddAsync(resource, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddMcpPromptAsync(McpPrompt prompt, CancellationToken cancellationToken = default) =>
        McpPrompts.AddAsync(prompt, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddMcpToolExecutionAsync(McpToolExecution execution, CancellationToken cancellationToken = default) =>
        McpToolExecutions.AddAsync(execution, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task RemovePluginInstallationAsync(Guid installationId, CancellationToken cancellationToken = default)
    {
        var installation = await PluginInstallations
            .Include(i => i.Settings)
            .Include(i => i.Logs)
            .FirstOrDefaultAsync(i => i.Id == installationId, cancellationToken);
        if (installation is not null)
        {
            PluginInstallations.Remove(installation);
        }
    }

    /// <inheritdoc />
    public async Task RemoveMcpServerAsync(Guid serverId, CancellationToken cancellationToken = default)
    {
        var server = await McpServers
            .Include(s => s.Tools)
            .Include(s => s.Resources)
            .Include(s => s.Prompts)
            .FirstOrDefaultAsync(s => s.Id == serverId, cancellationToken);
        if (server is not null)
        {
            McpServers.Remove(server);
        }
    }

    /// <inheritdoc />
    public async Task ClearMcpServerCapabilitiesAsync(Guid serverId, CancellationToken cancellationToken = default)
    {
        var tools = await McpTools.Where(t => t.McpServerId == serverId).ToListAsync(cancellationToken);
        var resources = await McpResources.Where(r => r.McpServerId == serverId).ToListAsync(cancellationToken);
        var prompts = await McpPrompts.Where(p => p.McpServerId == serverId).ToListAsync(cancellationToken);
        McpTools.RemoveRange(tools);
        McpResources.RemoveRange(resources);
        McpPrompts.RemoveRange(prompts);
    }

    /// <inheritdoc />
    public Task AddAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default) =>
        AuditEvents.AddAsync(auditEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddIdentityProviderAsync(IdentityProvider provider, CancellationToken cancellationToken = default) =>
        IdentityProviders.AddAsync(provider, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddScimMappingAsync(ScimMapping mapping, CancellationToken cancellationToken = default) =>
        ScimMappings.AddAsync(mapping, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddSecretReferenceAsync(SecretReference secret, CancellationToken cancellationToken = default) =>
        SecretReferences.AddAsync(secret, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddComplianceEventAsync(ComplianceEvent complianceEvent, CancellationToken cancellationToken = default) =>
        ComplianceEvents.AddAsync(complianceEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddSessionHistoryAsync(SessionHistory session, CancellationToken cancellationToken = default) =>
        SessionHistories.AddAsync(session, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddTrustedDeviceAsync(TrustedDevice device, CancellationToken cancellationToken = default) =>
        TrustedDevices.AddAsync(device, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationSecurityPolicyAsync(
        OrganizationSecurityPolicy policy,
        CancellationToken cancellationToken = default) =>
        OrganizationSecurityPolicies.AddAsync(policy, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationGovernancePolicyAsync(
        OrganizationGovernancePolicy policy,
        CancellationToken cancellationToken = default) =>
        OrganizationGovernancePolicies.AddAsync(policy, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationComplianceSettingsAsync(
        OrganizationComplianceSettings settings,
        CancellationToken cancellationToken = default) =>
        OrganizationComplianceSettings.AddAsync(settings, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddUserMfaEnrollmentAsync(UserMfaEnrollment enrollment, CancellationToken cancellationToken = default) =>
        UserMfaEnrollments.AddAsync(enrollment, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddSubscriptionPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default) =>
        SubscriptionPlans.AddAsync(plan, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPlanQuotaAsync(PlanQuota quota, CancellationToken cancellationToken = default) =>
        PlanQuotas.AddAsync(quota, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationSubscriptionAsync(
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default) =>
        OrganizationSubscriptions.AddAsync(subscription, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddUsageRecordAsync(UsageRecord record, CancellationToken cancellationToken = default) =>
        UsageRecords.AddAsync(record, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        Invoices.AddAsync(invoice, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProductLicenseAsync(ProductLicense license, CancellationToken cancellationToken = default) =>
        ProductLicenses.AddAsync(license, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOnboardingProgressAsync(OnboardingProgress progress, CancellationToken cancellationToken = default) =>
        OnboardingProgressRecords.AddAsync(progress, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddTelemetryPreferenceAsync(
        TelemetryPreference preference,
        CancellationToken cancellationToken = default) =>
        TelemetryPreferences.AddAsync(preference, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddBackupJobAsync(BackupJob job, CancellationToken cancellationToken = default) =>
        BackupJobs.AddAsync(job, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPlatformReleaseAsync(PlatformRelease release, CancellationToken cancellationToken = default) =>
        PlatformReleases.AddAsync(release, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddAiDeploymentAsync(AiDeployment deployment, CancellationToken cancellationToken = default) =>
        AiDeployments.AddAsync(deployment, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddDeploymentTemplateAsync(DeploymentTemplate template, CancellationToken cancellationToken = default) =>
        DeploymentTemplates.AddAsync(template, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddDeploymentModelAsync(DeploymentModel model, CancellationToken cancellationToken = default) =>
        DeploymentModels.AddAsync(model, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddDeploymentLogAsync(DeploymentLog log, CancellationToken cancellationToken = default) =>
        DeploymentLogs.AddAsync(log, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddRuntimeVersionAsync(RuntimeVersion version, CancellationToken cancellationToken = default) =>
        RuntimeVersions.AddAsync(version, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGpuCatalogEntryAsync(GpuCatalogEntry entry, CancellationToken cancellationToken = default) =>
        GpuCatalogEntries.AddAsync(entry, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddModelCatalogEntryAsync(ModelCatalogEntry entry, CancellationToken cancellationToken = default) =>
        ModelCatalogEntries.AddAsync(entry, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddDeploymentHealthAsync(DeploymentHealth health, CancellationToken cancellationToken = default) =>
        DeploymentHealthSnapshots.AddAsync(health, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddDeploymentHistoryAsync(DeploymentHistory history, CancellationToken cancellationToken = default) =>
        DeploymentHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task RemoveIdentityProviderAsync(Guid identityProviderId, CancellationToken cancellationToken = default)
    {
        var provider = await IdentityProviders.FirstOrDefaultAsync(p => p.Id == identityProviderId, cancellationToken);
        if (provider is not null)
        {
            IdentityProviders.Remove(provider);
        }
    }

    /// <inheritdoc />
    public async Task RemoveSecretReferenceAsync(Guid secretId, CancellationToken cancellationToken = default)
    {
        var secret = await SecretReferences.FirstOrDefaultAsync(s => s.Id == secretId, cancellationToken);
        if (secret is not null)
        {
            SecretReferences.Remove(secret);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAiInferenceProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var provider = await AiInferenceProviders
            .FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);

        if (provider is not null)
        {
            AiInferenceProviders.Remove(provider);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAiRoutingPolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var policy = await AiRoutingPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy is not null)
        {
            AiRoutingPolicies.Remove(policy);
        }
    }

    /// <inheritdoc />
    public async Task RemovePodPoolAsync(Guid poolId, CancellationToken cancellationToken = default)
    {
        await RemovePodPoolModelsAsync(poolId, cancellationToken);

        var members = await PodPoolMembers
            .Where(m => m.PodPoolId == poolId)
            .ToListAsync(cancellationToken);

        if (members.Count > 0)
        {
            PodPoolMembers.RemoveRange(members);
        }

        var pool = await PodPools.FirstOrDefaultAsync(p => p.Id == poolId, cancellationToken);
        if (pool is not null)
        {
            PodPools.Remove(pool);
        }
    }

    /// <inheritdoc />
    public async Task RemovePodPoolModelsAsync(Guid poolId, CancellationToken cancellationToken = default)
    {
        var models = await PodPoolModels
            .Where(m => m.PodPoolId == poolId)
            .ToListAsync(cancellationToken);

        if (models.Count > 0)
        {
            PodPoolModels.RemoveRange(models);
        }
    }

    /// <inheritdoc />
    public async Task RemovePodPoolMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var member = await PodPoolMembers.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
        if (member is not null)
        {
            PodPoolMembers.Remove(member);
        }
    }

    /// <inheritdoc />
    public async Task RemoveGatewayRouteAsync(Guid routeId, CancellationToken cancellationToken = default)
    {
        var route = await GatewayRoutes.FirstOrDefaultAsync(r => r.Id == routeId, cancellationToken);
        if (route is not null)
        {
            GatewayRoutes.Remove(route);
        }
    }

    /// <inheritdoc />
    public async Task RemovePodEndpointsAsync(Guid podId, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<PodEndpoint>()
            .Where(e => e.Entity.GpuPodId == podId)
            .ToList())
        {
            entry.State = EntityState.Detached;
        }

        var endpoints = await PodEndpoints
            .Where(e => e.GpuPodId == podId)
            .ToListAsync(cancellationToken);

        if (endpoints.Count == 0)
        {
            return;
        }

        PodEndpoints.RemoveRange(endpoints);
    }

    /// <inheritdoc />
    public Task AddPodEndpointAsync(PodEndpoint endpoint, CancellationToken cancellationToken = default) =>
        PodEndpoints.AddAsync(endpoint, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task RemovePodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default)
    {
        PodLifecycleLocks.Remove(lifecycleLock);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task RemoveGpuPodAsync(Guid podId, CancellationToken cancellationToken = default)
    {
        var pod = await GpuPods.FirstOrDefaultAsync(p => p.Id == podId, cancellationToken);
        if (pod is not null)
        {
            GpuPods.Remove(pod);
        }
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        EnsureAuditEventsAreImmutable();
        return base.SaveChanges();
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuditEventsAreImmutable();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
    }

    private void EnsureAuditEventsAreImmutable()
    {
        var illegal = ChangeTracker.Entries<AuditEvent>()
            .Where(e => e.State is EntityState.Modified or EntityState.Deleted)
            .Any();
        if (illegal)
        {
            throw new InvalidOperationException("AuditEvent records are immutable and cannot be updated or deleted.");
        }
    }
}
