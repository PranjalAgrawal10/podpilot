using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Application database context abstraction.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the refresh tokens set.
    /// </summary>
    IQueryable<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// Gets the organizations set.
    /// </summary>
    IQueryable<Organization> Organizations { get; }

    /// <summary>
    /// Gets the organization members set.
    /// </summary>
    IQueryable<OrganizationMember> OrganizationMembers { get; }

    /// <summary>
    /// Gets the invitations set.
    /// </summary>
    IQueryable<Invitation> Invitations { get; }

    /// <summary>
    /// Gets the permissions set.
    /// </summary>
    IQueryable<Permission> Permissions { get; }

    /// <summary>
    /// Gets the roles set.
    /// </summary>
    IQueryable<Role> Roles { get; }

    /// <summary>
    /// Gets the audit logs set.
    /// </summary>
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Gets the compute providers set.
    /// </summary>
    IQueryable<ComputeProvider> ComputeProviders { get; }

    /// <summary>
    /// Gets the provider credentials set.
    /// </summary>
    IQueryable<ProviderCredential> ProviderCredentials { get; }

    /// <summary>
    /// Gets the provider regions set.
    /// </summary>
    IQueryable<ProviderRegion> ProviderRegions { get; }

    /// <summary>
    /// Gets the provider GPUs set.
    /// </summary>
    IQueryable<ProviderGpu> ProviderGpus { get; }

    /// <summary>
    /// Gets the provider health snapshots set.
    /// </summary>
    IQueryable<ProviderHealth> ProviderHealthSnapshots { get; }

    /// <summary>
    /// Gets the provider health history set.
    /// </summary>
    IQueryable<ProviderHealthHistory> ProviderHealthHistory { get; }

    /// <summary>
    /// Gets the GPU pods set.
    /// </summary>
    IQueryable<GpuPod> GpuPods { get; }

    /// <summary>
    /// Gets the pod configurations set.
    /// </summary>
    IQueryable<PodConfiguration> PodConfigurations { get; }

    /// <summary>
    /// Gets the pod endpoints set.
    /// </summary>
    IQueryable<PodEndpoint> PodEndpoints { get; }

    /// <summary>
    /// Gets the pod status history set.
    /// </summary>
    IQueryable<PodStatusHistory> PodStatusHistory { get; }

    /// <summary>
    /// Gets the pod activities set.
    /// </summary>
    IQueryable<PodActivity> PodActivities { get; }

    /// <summary>
    /// Gets the pod lifecycle events set.
    /// </summary>
    IQueryable<PodLifecycleEvent> PodLifecycleEvents { get; }

    /// <summary>
    /// Gets the pod idle policies set.
    /// </summary>
    IQueryable<PodIdlePolicy> PodIdlePolicies { get; }

    /// <summary>
    /// Gets the pod lifecycle locks set.
    /// </summary>
    IQueryable<PodLifecycleLock> PodLifecycleLocks { get; }

    /// <summary>
    /// Gets the pod wake requests set.
    /// </summary>
    IQueryable<PodWakeRequest> PodWakeRequests { get; }

    /// <summary>
    /// Gets the gateway API keys set.
    /// </summary>
    IQueryable<GatewayApiKey> GatewayApiKeys { get; }

    /// <summary>
    /// Gets the gateway routes set.
    /// </summary>
    IQueryable<GatewayRoute> GatewayRoutes { get; }

    /// <summary>
    /// Gets the gateway requests set.
    /// </summary>
    IQueryable<GatewayRequest> GatewayRequests { get; }

    /// <summary>
    /// Gets the request queue entries set.
    /// </summary>
    IQueryable<RequestQueueEntry> RequestQueueEntries { get; }

    /// <summary>
    /// Gets the request executions set.
    /// </summary>
    IQueryable<RequestExecution> RequestExecutions { get; }

    /// <summary>
    /// Gets the scheduler events set.
    /// </summary>
    IQueryable<SchedulerEvent> SchedulerEvents { get; }

    /// <summary>
    /// Gets the AI models set.
    /// </summary>
    IQueryable<AiModel> AiModels { get; }

    /// <summary>
    /// Gets the model downloads set.
    /// </summary>
    IQueryable<ModelDownload> ModelDownloads { get; }

    /// <summary>
    /// Gets the model health history set.
    /// </summary>
    IQueryable<ModelHealthHistory> ModelHealthHistory { get; }

    /// <summary>
    /// Gets applied database migration audit records.
    /// </summary>
    IQueryable<DatabaseMigrationHistory> DatabaseMigrationHistory { get; }

    /// <summary>
    /// Gets the database seeder audit records.
    /// </summary>
    IQueryable<DatabaseSeedHistory> DatabaseSeedHistory { get; }

    /// <summary>
    /// Gets the pod pools set.
    /// </summary>
    IQueryable<PodPool> PodPools { get; }

    /// <summary>
    /// Gets the pod pool members set.
    /// </summary>
    IQueryable<PodPoolMember> PodPoolMembers { get; }

    /// <summary>
    /// Gets the pod pool models set.
    /// </summary>
    IQueryable<PodPoolModel> PodPoolModels { get; }

    /// <summary>
    /// Gets the scaling policies set.
    /// </summary>
    IQueryable<ScalingPolicy> ScalingPolicies { get; }

    /// <summary>
    /// Gets the scaling events set.
    /// </summary>
    IQueryable<ScalingEvent> ScalingEvents { get; }

    /// <summary>
    /// Gets the pod health metrics set.
    /// </summary>
    IQueryable<PodHealthMetric> PodHealthMetrics { get; }

    /// <summary>
    /// Gets the capacity snapshots set.
    /// </summary>
    IQueryable<CapacitySnapshot> CapacitySnapshots { get; }

    /// <summary>
    /// Gets the load balancer configs set.
    /// </summary>
    IQueryable<LoadBalancerConfig> LoadBalancerConfigs { get; }

    /// <summary>
    /// Gets the metrics snapshots set.
    /// </summary>
    IQueryable<MetricsSnapshot> MetricsSnapshots { get; }

    /// <summary>
    /// Gets the cost snapshots set.
    /// </summary>
    IQueryable<CostSnapshot> CostSnapshots { get; }

    /// <summary>
    /// Gets the usage statistics set.
    /// </summary>
    IQueryable<UsageStatistics> UsageStatistics { get; }

    /// <summary>
    /// Gets the alert history set.
    /// </summary>
    IQueryable<AlertHistory> AlertHistory { get; }

    /// <summary>
    /// Gets the system health history set.
    /// </summary>
    IQueryable<SystemHealthHistory> SystemHealthHistory { get; }

    /// <summary>
    /// Gets the AI inference providers set.
    /// </summary>
    IQueryable<AiInferenceProvider> AiInferenceProviders { get; }

    /// <summary>
    /// Gets the AI provider credentials set.
    /// </summary>
    IQueryable<AiProviderCredential> AiProviderCredentials { get; }

    /// <summary>
    /// Gets the AI provider models set.
    /// </summary>
    IQueryable<AiProviderModel> AiProviderModels { get; }

    /// <summary>
    /// Gets the AI provider health snapshots set.
    /// </summary>
    IQueryable<AiProviderHealth> AiProviderHealthSnapshots { get; }

    /// <summary>
    /// Gets the AI routing policies set.
    /// </summary>
    IQueryable<AiRoutingPolicy> AiRoutingPolicies { get; }

    /// <summary>
    /// Gets the AI failover events set.
    /// </summary>
    IQueryable<AiFailoverEvent> AiFailoverEvents { get; }

    /// <summary>
    /// Gets the model scores set.
    /// </summary>
    IQueryable<ModelScore> ModelScores { get; }

    /// <summary>
    /// Gets the latency history set.
    /// </summary>
    IQueryable<LatencyHistory> LatencyHistories { get; }

    /// <summary>
    /// Gets the cost history set.
    /// </summary>
    IQueryable<CostHistory> CostHistories { get; }

    /// <summary>
    /// Gets the routing events set.
    /// </summary>
    IQueryable<RoutingEvent> RoutingEvents { get; }

    /// <summary>Gets plugin definitions.</summary>
    IQueryable<PluginDefinition> PluginDefinitions { get; }

    /// <summary>Gets plugin installations.</summary>
    IQueryable<PluginInstallation> PluginInstallations { get; }

    /// <summary>Gets plugin settings.</summary>
    IQueryable<PluginSetting> PluginSettings { get; }

    /// <summary>Gets plugin logs.</summary>
    IQueryable<PluginLog> PluginLogs { get; }

    /// <summary>Gets MCP servers.</summary>
    IQueryable<McpServer> McpServers { get; }

    /// <summary>Gets MCP tools.</summary>
    IQueryable<McpTool> McpTools { get; }

    /// <summary>Gets MCP resources.</summary>
    IQueryable<McpResource> McpResources { get; }

    /// <summary>Gets MCP prompts.</summary>
    IQueryable<McpPrompt> McpPrompts { get; }

    /// <summary>Gets MCP tool executions.</summary>
    IQueryable<McpToolExecution> McpToolExecutions { get; }

    /// <summary>
    /// Adds an audit log entry.
    /// </summary>
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a refresh token.
    /// </summary>
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization.
    /// </summary>
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization member.
    /// </summary>
    Task AddOrganizationMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an invitation.
    /// </summary>
    Task AddInvitationAsync(Invitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a compute provider.
    /// </summary>
    Task AddComputeProviderAsync(ComputeProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider credential.
    /// </summary>
    Task AddProviderCredentialAsync(ProviderCredential credential, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider health snapshot.
    /// </summary>
    Task AddProviderHealthAsync(ProviderHealth health, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider health history entry.
    /// </summary>
    Task AddProviderHealthHistoryAsync(ProviderHealthHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider region.
    /// </summary>
    Task AddProviderRegionAsync(ProviderRegion region, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider GPU.
    /// </summary>
    Task AddProviderGpuAsync(ProviderGpu gpu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes cached regions and GPUs for a provider.
    /// </summary>
    Task ClearProviderCatalogAsync(Guid computeProviderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a compute provider.
    /// </summary>
    Task RemoveComputeProviderAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a GPU pod.
    /// </summary>
    Task AddGpuPodAsync(GpuPod pod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod status history entry.
    /// </summary>
    Task AddPodStatusHistoryAsync(PodStatusHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod activity entry.
    /// </summary>
    Task AddPodActivityAsync(PodActivity activity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod lifecycle event.
    /// </summary>
    Task AddPodLifecycleEventAsync(PodLifecycleEvent lifecycleEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod idle policy.
    /// </summary>
    Task AddPodIdlePolicyAsync(PodIdlePolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod wake request.
    /// </summary>
    Task AddPodWakeRequestAsync(PodWakeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod lifecycle lock.
    /// </summary>
    Task AddPodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a gateway API key.
    /// </summary>
    Task AddGatewayApiKeyAsync(GatewayApiKey apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a gateway route.
    /// </summary>
    Task AddGatewayRouteAsync(GatewayRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a gateway request.
    /// </summary>
    Task AddGatewayRequestAsync(GatewayRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a request queue entry.
    /// </summary>
    Task AddRequestQueueEntryAsync(RequestQueueEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a request execution record.
    /// </summary>
    Task AddRequestExecutionAsync(RequestExecution execution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a scheduler event.
    /// </summary>
    Task AddSchedulerEventAsync(SchedulerEvent schedulerEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI model.
    /// </summary>
    Task AddAiModelAsync(AiModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a model download.
    /// </summary>
    Task AddModelDownloadAsync(ModelDownload download, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a model health history entry.
    /// </summary>
    Task AddModelHealthHistoryAsync(ModelHealthHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod pool.
    /// </summary>
    Task AddPodPoolAsync(PodPool pool, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod pool member.
    /// </summary>
    Task AddPodPoolMemberAsync(PodPoolMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod pool model.
    /// </summary>
    Task AddPodPoolModelAsync(PodPoolModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a scaling policy.
    /// </summary>
    Task AddScalingPolicyAsync(ScalingPolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a scaling event.
    /// </summary>
    Task AddScalingEventAsync(ScalingEvent scalingEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod health metric.
    /// </summary>
    Task AddPodHealthMetricAsync(PodHealthMetric metric, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a capacity snapshot.
    /// </summary>
    Task AddCapacitySnapshotAsync(CapacitySnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a load balancer config.
    /// </summary>
    Task AddLoadBalancerConfigAsync(LoadBalancerConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a metrics snapshot.
    /// </summary>
    Task AddMetricsSnapshotAsync(MetricsSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a cost snapshot.
    /// </summary>
    Task AddCostSnapshotAsync(CostSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds usage statistics.
    /// </summary>
    Task AddUsageStatisticsAsync(UsageStatistics statistics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an alert history entry.
    /// </summary>
    Task AddAlertHistoryAsync(AlertHistory alert, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a system health history entry.
    /// </summary>
    Task AddSystemHealthHistoryAsync(SystemHealthHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI inference provider.
    /// </summary>
    Task AddAiInferenceProviderAsync(AiInferenceProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI provider credential.
    /// </summary>
    Task AddAiProviderCredentialAsync(AiProviderCredential credential, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI provider model catalog entry.
    /// </summary>
    Task AddAiProviderModelAsync(AiProviderModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI provider health snapshot.
    /// </summary>
    Task AddAiProviderHealthAsync(AiProviderHealth health, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI routing policy.
    /// </summary>
    Task AddAiRoutingPolicyAsync(AiRoutingPolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an AI failover event.
    /// </summary>
    Task AddAiFailoverEventAsync(AiFailoverEvent failoverEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a model score.
    /// </summary>
    Task AddModelScoreAsync(ModelScore score, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a latency history sample.
    /// </summary>
    Task AddLatencyHistoryAsync(LatencyHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a cost history sample.
    /// </summary>
    Task AddCostHistoryAsync(CostHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a routing event.
    /// </summary>
    Task AddRoutingEventAsync(RoutingEvent routingEvent, CancellationToken cancellationToken = default);

    /// <summary>Adds a plugin definition.</summary>
    Task AddPluginDefinitionAsync(PluginDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>Adds a plugin installation.</summary>
    Task AddPluginInstallationAsync(PluginInstallation installation, CancellationToken cancellationToken = default);

    /// <summary>Adds a plugin setting.</summary>
    Task AddPluginSettingAsync(PluginSetting setting, CancellationToken cancellationToken = default);

    /// <summary>Adds a plugin log.</summary>
    Task AddPluginLogAsync(PluginLog log, CancellationToken cancellationToken = default);

    /// <summary>Adds an MCP server.</summary>
    Task AddMcpServerAsync(McpServer server, CancellationToken cancellationToken = default);

    /// <summary>Adds an MCP tool.</summary>
    Task AddMcpToolAsync(McpTool tool, CancellationToken cancellationToken = default);

    /// <summary>Adds an MCP resource.</summary>
    Task AddMcpResourceAsync(McpResource resource, CancellationToken cancellationToken = default);

    /// <summary>Adds an MCP prompt.</summary>
    Task AddMcpPromptAsync(McpPrompt prompt, CancellationToken cancellationToken = default);

    /// <summary>Adds an MCP tool execution.</summary>
    Task AddMcpToolExecutionAsync(McpToolExecution execution, CancellationToken cancellationToken = default);

    /// <summary>Removes a plugin installation.</summary>
    Task RemovePluginInstallationAsync(Guid installationId, CancellationToken cancellationToken = default);

    /// <summary>Removes an MCP server.</summary>
    Task RemoveMcpServerAsync(Guid serverId, CancellationToken cancellationToken = default);

    /// <summary>Clears discovered MCP tools/resources/prompts for a server.</summary>
    Task ClearMcpServerCapabilitiesAsync(Guid serverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an AI inference provider.
    /// </summary>
    Task RemoveAiInferenceProviderAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an AI routing policy.
    /// </summary>
    Task RemoveAiRoutingPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a pod pool and its children.
    /// </summary>
    Task RemovePodPoolAsync(Guid poolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes pod pool models for a pool.
    /// </summary>
    Task RemovePodPoolModelsAsync(Guid poolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a pod pool member.
    /// </summary>
    Task RemovePodPoolMemberAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a gateway route.
    /// </summary>
    Task RemoveGatewayRouteAsync(Guid routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all endpoints for a GPU pod.
    /// </summary>
    Task RemovePodEndpointsAsync(Guid podId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod endpoint.
    /// </summary>
    Task AddPodEndpointAsync(PodEndpoint endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a pod lifecycle lock.
    /// </summary>
    Task RemovePodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a GPU pod.
    /// </summary>
    Task RemoveGpuPodAsync(Guid podId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
