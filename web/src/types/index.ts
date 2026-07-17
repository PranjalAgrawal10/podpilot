export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: Record<string, string[]>;
  correlationId?: string;
}

export type OrganizationRole = 'Owner' | 'Admin' | 'Developer' | 'Viewer';

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  currentOrganizationId?: string | null;
  currentOrganizationRole?: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  user: UserSummary;
  requiresMfa?: boolean;
  mfaToken?: string | null;
}

export interface OrganizationSummary {
  id: string;
  name: string;
  slug: string;
  role: string;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  organizations: OrganizationSummary[];
}

export interface Organization {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  logo?: string | null;
  ownerUserId: string;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  currentUserRole?: string | null;
}

export interface Member {
  id: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  status: string;
  joinedAt: string;
}

export interface Invitation {
  id: string;
  organizationId: string;
  email: string;
  role: string;
  status: string;
  expiresAt: string;
  token: string;
}

export interface CreateOrganizationRequest {
  name: string;
  description?: string | null;
  logo?: string | null;
}

export interface UpdateOrganizationRequest {
  name?: string | null;
  description?: string | null;
  logo?: string | null;
}

export interface SwitchOrganizationRequest {
  organizationId: string;
}

export interface AddMemberRequest {
  email: string;
  role: string;
}

export interface UpdateMemberRoleRequest {
  role: string;
}

export interface InviteMemberRequest {
  email: string;
  role: string;
}

export interface AcceptInvitationRequest {
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationName: string;
}

export interface HealthResponse {
  status: string;
  checks: Record<string, { status: string; description?: string; duration: string }>;
  totalDuration: string;
}

export type PodStatus =
  | 'Creating'
  | 'BuildingPending'
  | 'Starting'
  | 'Running'
  | 'Stopping'
  | 'Stopped'
  | 'Restarting'
  | 'Deleting'
  | 'Deleted'
  | 'Failed'
  | 'Unknown';

export interface PodEndpoint {
  port: number;
  protocol: string;
  publicPort?: number | null;
  url?: string | null;
}

export interface PodStatusHistoryEntry {
  status: PodStatus;
  recordedAt: string;
  message?: string | null;
}

export interface PodConfiguration {
  templateId?: string | null;
  templateName?: string | null;
  imageName: string;
  containerDiskGb: number;
  volumeDiskGb: number;
  volumeMountPath: string;
  gpuCount: number;
  environmentVariables: Record<string, string>;
  ports: string[];
  enablePublicIp: boolean;
}

export interface Pod {
  id: string;
  organizationId: string;
  providerId: string;
  providerName: string;
  providerType: ProviderType;
  providerPodId?: string | null;
  name: string;
  description?: string | null;
  status: PodStatus;
  gpuType: GpuType;
  gpuId: string;
  region: string;
  templateId?: string | null;
  imageName: string;
  containerDisk: number;
  volumeDisk: number;
  publicIp?: string | null;
  endpoint?: string | null;
  isPublic: boolean;
  hourlyCost?: number | null;
  createdAt: string;
  updatedAt?: string | null;
  lastStartedAt?: string | null;
  lastStoppedAt?: string | null;
  lastSyncedAt?: string | null;
  endpoints: PodEndpoint[];
  statusHistory: PodStatusHistoryEntry[];
  configuration?: PodConfiguration | null;
}

export interface PodActivity {
  id: string;
  activityType: string;
  timestamp: string;
  source: string;
  userId?: string | null;
  metadata?: string | null;
}

export interface PodLifecycleEvent {
  id: string;
  eventType: string;
  timestamp: string;
  source: string;
  userId?: string | null;
  message?: string | null;
  metadata?: string | null;
}

export interface PodIdlePolicy {
  podId: string;
  idleTimeoutMinutes: number;
  gracePeriodMinutes: number;
  autoShutdownEnabled: boolean;
  autoWakeEnabled: boolean;
  minimumRunningTimeMinutes: number;
  idleDetectedAt?: string | null;
}

export interface PodLifecycleSummary {
  podId: string;
  status: string;
  runningTimeMinutes: number;
  idleTimeMinutes: number;
  lastActivityAt?: string | null;
  nextShutdownAt?: string | null;
  autoWakeEnabled: boolean;
  autoShutdownEnabled: boolean;
  isIdle: boolean;
  policy: PodIdlePolicy;
}

export interface UpdatePodIdlePolicyRequest {
  idleTimeoutMinutes: number;
  gracePeriodMinutes: number;
  autoShutdownEnabled: boolean;
  autoWakeEnabled: boolean;
  minimumRunningTimeMinutes: number;
}

export interface PodWakeResult {
  success: boolean;
  queued: boolean;
  wakeRequestId?: string | null;
  status?: string | null;
  errorMessage?: string | null;
}

export interface PodShutdownResult {
  success: boolean;
  status?: string | null;
  errorMessage?: string | null;
}

export interface CreatePodRequest {
  providerId: string;
  name: string;
  description?: string | null;
  gpuId: string;
  gpuType: GpuType;
  region: string;
  templateId?: string | null;
  templateName?: string | null;
  imageName: string;
  containerDiskGb: number;
  volumeDiskGb: number;
  volumeMountPath?: string;
  gpuCount?: number;
  environmentVariables?: Record<string, string>;
  ports?: string[];
  enablePublicIp?: boolean;
}

export interface UpdatePodRequest {
  name?: string | null;
  description?: string | null;
}

export const GPU_TYPES: GpuType[] = [
  'RTX4090',
  'RTX5090',
  'A100',
  'H100',
  'L40S',
  'A40',
  'V100',
  'Custom',
];

export const POD_STATUSES: PodStatus[] = [
  'Creating',
  'BuildingPending',
  'Starting',
  'Running',
  'Stopping',
  'Stopped',
  'Restarting',
  'Deleting',
  'Deleted',
  'Failed',
  'Unknown',
];

export type ProviderType =
  | 'RunPod'
  | 'Vast'
  | 'Lambda'
  | 'Azure'
  | 'AWS'
  | 'GoogleCloud'
  | 'Kubernetes';

export type GpuType =
  | 'RTX4090'
  | 'RTX5090'
  | 'A100'
  | 'H100'
  | 'L40S'
  | 'A40'
  | 'V100'
  | 'Custom';

export type ProviderConnectionStatus = 'Connected' | 'Disconnected' | 'Degraded' | 'Unknown';

export interface Provider {
  id: string;
  organizationId: string;
  name: string;
  providerType: ProviderType;
  displayName: string;
  description?: string | null;
  defaultRegion?: string | null;
  isEnabled: boolean;
  isValidated: boolean;
  lastValidatedAt?: string | null;
  connectionStatus?: ProviderConnectionStatus;
  createdAt: string;
  updatedAt?: string | null;
}

export interface ProviderAccountInfo {
  accountId?: string | null;
  accountName?: string | null;
  email?: string | null;
  balance?: number | null;
  currency?: string | null;
}

export interface ProviderRegion {
  id: string;
  regionId: string;
  name: string;
  displayName?: string | null;
  isAvailable: boolean;
}

export interface ProviderGpu {
  gpuId: string;
  gpuType: GpuType;
  name: string;
  memoryGb?: number | null;
  isAvailable: boolean;
}

export interface ProviderTemplate {
  templateId: string;
  name: string;
  description?: string | null;
  imageName?: string | null;
}

export interface ProviderHealth {
  providerId: string;
  status: ProviderConnectionStatus;
  message?: string | null;
  lastCheckedAt: string;
  responseTimeMs?: number | null;
  isHealthy: boolean;
}

export interface ProviderValidationResult {
  isValid: boolean;
  message?: string | null;
  connectionStatus: ProviderConnectionStatus;
  account?: ProviderAccountInfo | null;
  regions: ProviderRegion[];
  gpus: ProviderGpu[];
  templates: ProviderTemplate[];
}

export interface CreateProviderRequest {
  name: string;
  providerType: ProviderType;
  displayName: string;
  description?: string | null;
  apiKey: string;
  defaultRegion?: string | null;
}

export interface UpdateProviderRequest {
  name?: string | null;
  displayName?: string | null;
  description?: string | null;
  apiKey?: string | null;
  defaultRegion?: string | null;
  isEnabled?: boolean | null;
}

export interface ValidateProviderRequest {
  providerType: ProviderType;
  apiKey: string;
  defaultRegion?: string | null;
}

export const PROVIDER_TYPES: ProviderType[] = [
  'RunPod',
  'Vast',
  'Lambda',
  'Azure',
  'AWS',
  'GoogleCloud',
  'Kubernetes',
];

export const PROVIDER_TYPE_LABELS: Record<ProviderType, string> = {
  RunPod: 'RunPod',
  Vast: 'Vast.ai',
  Lambda: 'Lambda Labs',
  Azure: 'Azure GPU VM',
  AWS: 'AWS EC2 GPU',
  GoogleCloud: 'Google Cloud GPU',
  Kubernetes: 'Kubernetes Cluster',
};

export type ThemeMode = 'light' | 'dark';

export const PERMISSIONS = {
  OrganizationRead: 'Organization.Read',
  OrganizationUpdate: 'Organization.Update',
  OrganizationDelete: 'Organization.Delete',
  PodRead: 'Pod.Read',
  PodCreate: 'Pod.Create',
  PodUpdate: 'Pod.Update',
  PodDelete: 'Pod.Delete',
  ProviderRead: 'Provider.Read',
  ProviderCreate: 'Provider.Create',
  ProviderUpdate: 'Provider.Update',
  ProviderDelete: 'Provider.Delete',
  ModelRead: 'Model.Read',
  ModelPull: 'Model.Pull',
  ModelDelete: 'Model.Delete',
  ModelManage: 'Model.Manage',
  GatewayRead: 'Gateway.Read',
  GatewayManage: 'Gateway.Manage',
  DashboardView: 'Dashboard.View',
  BillingView: 'Billing.View',
  BillingRead: 'Billing.Read',
  BillingManage: 'Billing.Manage',
  LicenseRead: 'License.Read',
  LicenseManage: 'License.Manage',
  BackupRead: 'Backup.Read',
  BackupManage: 'Backup.Manage',
  MemberRead: 'Member.Read',
  MemberManage: 'Member.Manage',
  MemberRoleUpdate: 'Member.RoleUpdate',
  InvitationCreate: 'Invitation.Create',
  OrchestratorRead: 'Orchestrator.Read',
  OrchestratorManage: 'Orchestrator.Manage',
  ObservabilityRead: 'Observability.Read',
  ObservabilityExport: 'Observability.Export',
  AiProviderRead: 'AiProvider.Read',
  AiProviderCreate: 'AiProvider.Create',
  AiProviderUpdate: 'AiProvider.Update',
  AiProviderDelete: 'AiProvider.Delete',
  RoutingRead: 'Routing.Read',
  RoutingManage: 'Routing.Manage',
  PluginRead: 'Plugin.Read',
  PluginManage: 'Plugin.Manage',
  McpRead: 'Mcp.Read',
  McpManage: 'Mcp.Manage',
  SecurityRead: 'Security.Read',
  SecurityManage: 'Security.Manage',
  AuditRead: 'Audit.Read',
  SecretsRead: 'Secrets.Read',
  SecretsManage: 'Secrets.Manage',
  PolicyRead: 'Policy.Read',
  PolicyManage: 'Policy.Manage',
  ComplianceRead: 'Compliance.Read',
  ComplianceManage: 'Compliance.Manage',
  DeploymentRead: 'Deployment.Read',
  DeploymentManage: 'Deployment.Manage',
} as const;

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];

export interface GatewayApiKey {
  id: string;
  name: string;
  keyPrefix: string;
  keyType: string;
  isRevoked: boolean;
  expiresAt?: string | null;
  rateLimitPerMinute: number;
  rateLimitPerDay: number;
  plaintextKey?: string | null;
  createdAt: string;
}

export interface GatewayRoute {
  id: string;
  gpuPodId: string;
  podName: string;
  modelName: string;
  isDefault: boolean;
}

export interface GatewayStats {
  activeRequests: number;
  streamingRequests: number;
  waitingPods: number;
  averageLatencyMs: number;
  recentErrors: number;
}

export interface GatewayRequestSummary {
  id: string;
  gpuPodId: string;
  path: string;
  model?: string | null;
  status: string;
  wakeTriggered: boolean;
  isStreaming: boolean;
  totalLatencyMs?: number | null;
  startedAt: string;
  completedAt?: string | null;
}

export interface AiModel {
  id: string;
  organizationId: string;
  podId: string;
  podName: string;
  name: string;
  tag: string;
  fullName: string;
  family?: string | null;
  size: number;
  quantization?: string | null;
  contextLength?: number | null;
  parameters?: string | null;
  license?: string | null;
  isDefault: boolean;
  status: string;
  lastUsed?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface AiModelDetail extends AiModel {
  healthHistory: ModelHealthRecord[];
  downloads: ModelDownload[];
}

export interface ModelDownload {
  id: string;
  modelId: string;
  modelName: string;
  podId: string;
  progress: number;
  status: string;
  downloadSpeed?: number | null;
  startedAt: string;
  completedAt?: string | null;
  errorMessage?: string | null;
}

export interface ModelHealthRecord {
  id: string;
  modelId: string;
  modelName: string;
  podId: string;
  status: string;
  responseTime?: number | null;
  lastChecked: string;
  errorMessage?: string | null;
}

export interface ModelDashboard {
  installedModels: number;
  downloadingModels: number;
  defaultModel?: string | null;
  storageUsedBytes: number;
  ollamaVersion?: string | null;
  ollamaDetected: boolean;
  healthyModels: number;
  unhealthyModels: number;
}

export type PodPoolType = 'Development' | 'Production' | 'Testing' | 'Custom';

export type LoadBalancingStrategy =
  | 'RoundRobin'
  | 'LeastBusy'
  | 'LeastQueue'
  | 'LowestLatency'
  | 'Weighted'
  | 'StickySession';

export const POD_POOL_TYPES: PodPoolType[] = ['Development', 'Production', 'Testing', 'Custom'];

export const LOAD_BALANCING_STRATEGIES: LoadBalancingStrategy[] = [
  'RoundRobin',
  'LeastBusy',
  'LeastQueue',
  'LowestLatency',
  'Weighted',
  'StickySession',
];

export interface ScalingPolicy {
  id?: string;
  name: string;
  minPods: number;
  maxPods: number;
  maxQueueLength: number;
  maxLatencyMs: number;
  scaleUpThreshold: number;
  scaleDownThreshold: number;
  warmStandbyCount: number;
  minRuntimeMinutes: number;
  autoScaleUpEnabled: boolean;
  autoScaleDownEnabled: boolean;
  evaluationIntervalSeconds?: number;
}

export interface PodPoolMember {
  id: string;
  gpuPodId: string;
  podName: string;
  podStatus: string;
  state: string;
  weight: number;
  isWarmStandby: boolean;
  activeStreams: number;
  joinedAt: string;
  lastHealthCheckAt?: string | null;
}

export interface PodPool {
  id: string;
  organizationId: string;
  name: string;
  description?: string | null;
  poolType: PodPoolType | string;
  isDefault: boolean;
  isActive: boolean;
  providerId?: string | null;
  gpuId?: string | null;
  gpuType?: string | null;
  region?: string | null;
  templateId?: string | null;
  imageName?: string | null;
  scalingPolicyId?: string | null;
  scalingPolicy?: ScalingPolicy | null;
  models: string[];
  members: PodPoolMember[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreatePodPoolRequest {
  name: string;
  description?: string | null;
  poolType?: PodPoolType | string;
  isDefault?: boolean;
  providerId?: string | null;
  gpuId?: string | null;
  gpuType?: string | null;
  region?: string | null;
  templateId?: string | null;
  imageName?: string | null;
  models?: string[];
  podIds?: string[];
  scalingPolicy?: ScalingPolicy;
}

export interface UpdatePodPoolRequest {
  name?: string | null;
  description?: string | null;
  poolType?: PodPoolType | string | null;
  isDefault?: boolean | null;
  isActive?: boolean | null;
  models?: string[];
  podIds?: string[];
  scalingPolicy?: ScalingPolicy;
}

export interface OrchestratorStatus {
  poolCount: number;
  runningPods: number;
  healthyPods: number;
  drainingPods: number;
  failedPods: number;
  queueLength: number;
  averageLatencyMs: number;
  requestsPerSecond: number;
}

export interface PoolScalingStatus {
  poolId: string;
  poolName: string;
  currentPods: number;
  minPods: number;
  maxPods: number;
  warmStandbyCount: number;
  utilization: number;
  scaleUpRecommended: boolean;
  scaleDownRecommended: boolean;
}

export interface ScalingEvent {
  id: string;
  podPoolId?: string | null;
  gpuPodId?: string | null;
  direction: string;
  triggerType: string;
  reason: string;
  success: boolean;
  errorMessage?: string | null;
  occurredAt: string;
  podCountBefore: number;
  podCountAfter: number;
}

export interface AutoScalerStatus {
  pools: PoolScalingStatus[];
  recentEvents: ScalingEvent[];
}

export interface CapacityInfo {
  organizationId: string;
  poolId?: string | null;
  currentCapacity: number;
  projectedCapacity: number;
  remainingCapacity: number;
  maximumThroughput: number;
  suggestedScale: number;
  totalPods: number;
  healthyPods: number;
  busyPods: number;
  queueLength: number;
  averageWaitTimeMs: number;
  averageLatencyMs: number;
  gpuUtilizationPercent: number;
  concurrentStreams: number;
}

export interface LoadBalancerConfig {
  strategy: LoadBalancingStrategy | string;
  stickySessionsEnabled: boolean;
  stickySessionTtlMinutes: number;
}

export interface UpdateLoadBalancerConfigRequest {
  strategy: LoadBalancingStrategy | string;
  stickySessionsEnabled: boolean;
  stickySessionTtlMinutes: number;
}

export interface PodHealthMetric {
  id: string;
  gpuPodId: string;
  recordedAt: string;
  gpuHealthy: boolean;
  ollamaHealthy: boolean;
  modelsHealthy: boolean;
  latencyMs: number;
  gpuUtilizationPercent?: number | null;
  memoryUsedBytes?: number | null;
  diskUsedBytes?: number | null;
  networkHealthy: boolean;
  state: string;
  errorMessage?: string | null;
}

export interface ManualScaleRequest {
  poolId: string;
  reason?: string | null;
}

export interface ScalingActionResult {
  poolId: string;
  direction: string;
  success: boolean;
  podId?: string | null;
  reason: string;
  errorMessage?: string | null;
}

export type MetricsPeriod = 'Hourly' | 'Daily' | 'Weekly' | 'Monthly';

export type ObservabilityExportFormat = 'csv' | 'json' | 'excel';

export type ObservabilityExportType = 'metrics' | 'cost' | 'usage' | 'alerts' | 'health';

export interface MetricsSnapshot {
  id: string;
  recordedAt: string;
  providerId?: string | null;
  gpuPodId?: string | null;
  modelName?: string | null;
  gpuUtilizationPercent: number;
  gpuMemoryUsedBytes?: number | null;
  gpuMemoryTotalBytes?: number | null;
  cpuUtilizationPercent: number;
  memoryUsedBytes?: number | null;
  memoryTotalBytes?: number | null;
  diskUsedBytes?: number | null;
  diskTotalBytes?: number | null;
  networkInBytes: number;
  networkOutBytes: number;
  temperatureCelsius?: number | null;
  powerWatts?: number | null;
  activeStreams: number;
  queueSize: number;
  inferenceCount: number;
  tokensGenerated: number;
  averageLatencyMs: number;
  errorRate: number;
}

export interface LiveMetrics {
  capturedAt: string;
  gpuUtilizationPercent: number;
  cpuUtilizationPercent: number;
  activeStreams: number;
  queueSize: number;
  requestsPerSecond: number;
  averageLatencyMs: number;
  errorRate: number;
  runningPods: number;
  healthyPods: number;
  failedPods: number;
  stoppedPods: number;
  modelsInstalled: number;
  gpuMemoryUsedBytes?: number | null;
  gpuMemoryTotalBytes?: number | null;
  inferenceCountLastHour: number;
  tokensGeneratedLastHour: number;
}

export interface PodCostBreakdown {
  podId: string;
  podName: string;
  hourlyCost: number;
  periodCost: number;
}

export interface ProviderCostBreakdown {
  providerId: string;
  providerName: string;
  hourlyCost: number;
  periodCost: number;
}

export interface ModelCostBreakdown {
  modelName: string;
  hourlyCost: number;
  periodCost: number;
  requestCount: number;
}

export interface CostSummary {
  period: string;
  calculatedAt: string;
  hourlyCost: number;
  dailyCost: number;
  weeklyCost: number;
  monthlyCost: number;
  projectedMonthlyCost: number;
  autoShutdownSavings: number;
  podBreakdowns: PodCostBreakdown[];
  providerBreakdowns: ProviderCostBreakdown[];
  modelBreakdowns: ModelCostBreakdown[];
}

export interface ModelUsageBreakdown {
  modelName: string;
  requestCount: number;
  tokenCount: number;
  averageLatencyMs: number;
}

export interface ProviderUsageBreakdown {
  providerId: string;
  providerName: string;
  requestCount: number;
  inferenceCount: number;
}

export interface PodUsageBreakdown {
  podId: string;
  podName: string;
  requestCount: number;
  uptimeSeconds: number;
}

export interface AnalyticsSummary {
  period: string;
  totalRequests: number;
  totalTokens: number;
  totalInferences: number;
  averageLatencyMs: number;
  errorRate: number;
  totalUptimeSeconds: number;
  modelBreakdowns: ModelUsageBreakdown[];
  providerBreakdowns: ProviderUsageBreakdown[];
  podBreakdowns: PodUsageBreakdown[];
}

export interface ComponentHealth {
  component: string;
  status: string;
  message: string;
  relatedEntityId?: string | null;
}

export interface SystemHealth {
  checkedAt: string;
  overallStatus: string;
  components: ComponentHealth[];
}

export interface PodHealthEntry {
  podId: string;
  podName: string;
  status: string;
  gpuHealthy: boolean;
  ollamaHealthy: boolean;
  modelsHealthy: boolean;
  latencyMs: number;
  gpuUtilizationPercent?: number | null;
  errorMessage?: string | null;
  lastCheckedAt?: string | null;
}

export interface PodHealthOverview {
  checkedAt: string;
  totalPods: number;
  healthyPods: number;
  degradedPods: number;
  unhealthyPods: number;
  pods: PodHealthEntry[];
}

export interface ProviderHealthEntry {
  providerId: string;
  providerName: string;
  status: string;
  responseTimeMs?: number | null;
  errorMessage?: string | null;
  lastCheckedAt?: string | null;
}

export interface ProviderHealthOverview {
  checkedAt: string;
  totalProviders: number;
  healthyProviders: number;
  unhealthyProviders: number;
  providers: ProviderHealthEntry[];
}

export interface ObservabilityAlert {
  id: string;
  raisedAt: string;
  resolvedAt?: string | null;
  alertType: string;
  severity: string;
  title: string;
  message: string;
  providerId?: string | null;
  gpuPodId?: string | null;
  modelName?: string | null;
  isActive: boolean;
}

export interface ObservabilityFilters {
  providerId?: string;
  podId?: string;
  model?: string;
  from?: string;
  to?: string;
  period?: MetricsPeriod;
}

export type AiProviderKind =
  | 'Ollama'
  | 'Vllm'
  | 'LlamaCpp'
  | 'OpenAi'
  | 'Anthropic'
  | 'OpenRouter'
  | 'AzureOpenAi'
  | 'GoogleGemini'
  | 'Groq'
  | 'TogetherAi'
  | 'FireworksAi'
  | 'DeepInfra';

export type AiFailoverStrategy = 'None' | 'RetryThenFailover' | 'ImmediateFailover';

export interface AiProvider {
  id: string;
  organizationId: string;
  name: string;
  displayName: string;
  description?: string | null;
  providerKind: AiProviderKind | string;
  baseUrl?: string | null;
  deploymentName?: string | null;
  apiVersion?: string | null;
  isEnabled: boolean;
  isValidated: boolean;
  lastValidatedAt?: string | null;
  priority: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateAiProviderRequest {
  name: string;
  displayName: string;
  description?: string;
  providerKind: string;
  apiKey: string;
  baseUrl?: string;
  deploymentName?: string;
  apiVersion?: string;
  isEnabled?: boolean;
  priority?: number;
}

export interface UpdateAiProviderRequest {
  name: string;
  displayName: string;
  description?: string;
  apiKey?: string;
  baseUrl?: string;
  deploymentName?: string;
  apiVersion?: string;
  isEnabled: boolean;
  priority: number;
}

export interface AiProviderModel {
  id: string;
  organizationId: string;
  aiProviderId: string;
  providerKind: string;
  providerDisplayName: string;
  modelName: string;
  displayName?: string | null;
  contextLength?: number | null;
  parameters?: string | null;
  supportsStreaming: boolean;
  supportsVision: boolean;
  supportsTools: boolean;
  supportsEmbeddings: boolean;
  inputCostPerMillionTokens?: number | null;
  outputCostPerMillionTokens?: number | null;
  isEnabled: boolean;
  syncedAt: string;
}

export interface AiProviderHealth {
  providerId: string;
  status: string;
  latencyMs?: number | null;
  errorRate: number;
  errorMessage?: string | null;
  lastCheckedAt: string;
  consecutiveFailures: number;
}

export interface AiRoutingPolicy {
  id: string;
  organizationId: string;
  name: string;
  modelName?: string | null;
  primaryProviderId: string;
  primaryProviderDisplayName?: string | null;
  fallbackProviderIds: string[];
  failoverStrategy: string;
  maxRetries: number;
  isEnabled: boolean;
  isDefault: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateAiRoutingPolicyRequest {
  name: string;
  modelName?: string;
  primaryProviderId: string;
  fallbackProviderIds?: string[];
  failoverStrategy?: string;
  maxRetries?: number;
  isEnabled?: boolean;
  isDefault?: boolean;
}

export interface AiProviderDashboard {
  connectedProviders: number;
  totalProviders: number;
  availableModels: number;
  unhealthyProviders: number;
  streamingSessions: number;
  averageLatencyMs: number;
  averageErrorRate: number;
}

export interface AiProviderKindMetadata {
  providerKind: string;
  displayName: string;
  defaultBaseUrl: string;
  requiresBaseUrl: boolean;
  requiresApiKey: boolean;
  isOpenAiCompatible: boolean;
}

export interface AiProviderValidationResult {
  isValid: boolean;
  message?: string | null;
}

export type RoutingStrategy =
  | 'LowestCost'
  | 'LowestLatency'
  | 'HighestAccuracy'
  | 'Balanced'
  | 'ProviderPriority'
  | 'CustomRules'
  | 'OrganizationRules';

export interface RoutingDashboard {
  currentModel?: string | null;
  currentProvider?: string | null;
  currentProviderId?: string | null;
  strategy: string;
  estimatedCostUsd: number;
  estimatedLatencyMs: number;
  fallbackCount: number;
  mostUsedModels: { modelName: string; count: number }[];
  providerRanking: {
    providerId: string;
    providerName: string;
    score: number;
    latencyMs?: number | null;
    availabilityScore: number;
  }[];
}

export interface RoutingPolicySettings {
  id: string;
  name: string;
  strategy: string;
  costWeight: number;
  latencyWeight: number;
  reliabilityWeight: number;
  contextWeight: number;
  featuresWeight: number;
  availabilityWeight: number;
  maxRetries: number;
  failoverStrategy: string;
  isDefault: boolean;
  primaryProviderId?: string | null;
  fallbackProviderIds: string[];
  preferredTaskTypes: string[];
  customRulesJson?: string | null;
}

export interface UpdateRoutingPolicySettingsRequest {
  strategy: string;
  costWeight: number;
  latencyWeight: number;
  reliabilityWeight: number;
  contextWeight: number;
  featuresWeight: number;
  availabilityWeight: number;
  maxRetries: number;
  failoverStrategy: string;
  primaryProviderId?: string | null;
  fallbackProviderIds?: string[];
  preferredTaskTypes?: string[];
  customRulesJson?: string | null;
}

export interface RankedModel {
  providerId: string;
  providerName: string;
  modelId: string;
  modelName: string;
  strategy: string;
  overallScore: number;
  costScore: number;
  latencyScore: number;
  reliabilityScore: number;
  contextScore: number;
  featuresScore: number;
  availabilityScore: number;
  scoredAt: string;
}

export interface RoutingHistoryItem {
  id: string;
  taskType: string;
  complexity: string;
  strategy: string;
  selectedProviderId?: string | null;
  selectedProviderName?: string | null;
  selectedModelName?: string | null;
  overallScore?: number | null;
  estimatedCostUsd: number;
  estimatedLatencyMs: number;
  fallbackCount: number;
  isSimulation: boolean;
  decisionReason?: string | null;
  decidedAt: string;
}

export interface SimulateRoutingRequest {
  prompt: string;
  strategy?: string;
  modelHint?: string;
  path?: string;
}

export interface SimulateRoutingResponse {
  taskType: string;
  complexity: string;
  strategy: string;
  predictedProvider?: string | null;
  predictedProviderId?: string | null;
  predictedModel?: string | null;
  estimatedCostUsd: number;
  estimatedLatencyMs: number;
  overallScore?: number | null;
  estimatedInputTokens: number;
  estimatedOutputTokens: number;
  decisionReason: string;
  rankedAlternatives: RankedModel[];
}

export interface Plugin {
  id: string;
  installationId?: string | null;
  packageId: string;
  name: string;
  version: string;
  pluginType: string;
  description?: string | null;
  publisher: string;
  isFirstParty: boolean;
  status?: string | null;
  isHealthy?: boolean | null;
  healthMessage?: string | null;
  requiredPermissions: string[];
  grantedPermissions: string[];
  enabledAt?: string | null;
  createdAt: string;
}

export interface InstallPluginRequest {
  packageId: string;
  grantedPermissions?: string[];
}

export interface UpdatePluginRequest {
  grantedPermissions?: string[];
}

export interface PluginSetting {
  key: string;
  value?: string | null;
  isSecret: boolean;
  hasValue: boolean;
}

export interface UpdatePluginSettingsRequest {
  settings: Record<string, string>;
  secretKeys?: string[];
}

export interface PluginDashboard {
  installedPlugins: number;
  enabledPlugins: number;
  connectedMcpServers: number;
  availableTools: number;
  unhealthyPlugins: number;
  recentExecutions: number;
}

export interface McpServer {
  id: string;
  organizationId: string;
  name: string;
  version: string;
  serverKind: string;
  endpoint: string;
  authScheme: string;
  hasCredential: boolean;
  status: string;
  isEnabled: boolean;
  timeoutSeconds: number;
  maxRetries: number;
  supportsStreaming: boolean;
  lastCheckedAt?: string | null;
  lastError?: string | null;
  toolCount: number;
  resourceCount: number;
  createdAt: string;
}

export interface CreateMcpServerRequest {
  name: string;
  version?: string;
  serverKind: string;
  endpoint: string;
  authScheme?: string;
  credential?: string | null;
  timeoutSeconds?: number;
  maxRetries?: number;
  supportsStreaming?: boolean;
  discoverOnCreate?: boolean;
}

export interface McpTool {
  id: string;
  mcpServerId: string;
  serverName: string;
  name: string;
  description?: string | null;
  inputSchemaJson?: string | null;
  isEnabled: boolean;
  discoveredAt: string;
}

export interface McpResource {
  id: string;
  mcpServerId: string;
  serverName: string;
  uri: string;
  name: string;
  mimeType?: string | null;
  description?: string | null;
  discoveredAt: string;
}

export interface McpServerKind {
  serverKind: string;
  displayName: string;
  description: string;
  defaultEndpoint?: string | null;
  defaultAuthScheme: string;
  requiresCredential: boolean;
}

export interface ExecuteMcpToolRequest {
  serverId?: string | null;
  toolName: string;
  argumentsJson?: string;
}

export interface ExecuteMcpToolResponse {
  succeeded: boolean;
  contentJson: string;
  errorMessage?: string | null;
  durationMs: number;
}

export interface IdentityProvider {
  id: string;
  name: string;
  providerKind: string;
  protocol: string;
  isEnabled: boolean;
  issuer?: string | null;
  clientId?: string | null;
  hasClientSecret: boolean;
  scopes: string;
  createdAt: string;
}

export interface CreateIdentityProviderRequest {
  name: string;
  providerKind?: string;
  protocol?: string;
  clientId?: string | null;
  clientSecret?: string | null;
  issuer?: string | null;
  authorizationEndpoint?: string | null;
  tokenEndpoint?: string | null;
  jwksUri?: string | null;
  samlEntityId?: string | null;
  samlSsoUrl?: string | null;
  samlCertificate?: string | null;
  scopes?: string;
  isEnabled?: boolean;
}

export interface BeginSsoRequest {
  organizationId: string;
  identityProviderId: string;
  redirectUri: string;
}

export interface SsoChallengeResponse {
  authorizationUrl: string;
  state: string;
}

export interface CompleteSsoRequest {
  organizationId: string;
  identityProviderId: string;
  code?: string | null;
  state?: string | null;
  samlResponse?: string | null;
  redirectUri: string;
}

export interface MfaRequest {
  code?: string | null;
  mfaToken?: string | null;
  action?: string;
}

export interface MfaEnrollmentResponse {
  sharedSecret: string;
  otpAuthUri: string;
}

export interface Secret {
  id: string;
  name: string;
  secretKind: string;
  backendKind: string;
  expiresAt?: string | null;
  lastRotatedAt?: string | null;
  lastAccessedAt?: string | null;
  isEnabled: boolean;
  version: number;
  createdAt: string;
}

export interface CreateSecretRequest {
  name: string;
  secretKind?: string;
  backendKind?: string;
  value: string;
  expiresAt?: string | null;
}

export interface UpdateSecretRequest {
  name?: string | null;
  value?: string | null;
  expiresAt?: string | null;
  isEnabled?: boolean | null;
}

export interface AuditEvent {
  id: string;
  organizationId?: string | null;
  userId?: string | null;
  actorEmail?: string | null;
  category: string;
  eventType: string;
  entityType?: string | null;
  entityId?: string | null;
  summary: string;
  ipAddress?: string | null;
  occurredAt: string;
}

export interface ListAuditEventsParams {
  category?: string;
  eventType?: string;
  fromUtc?: string;
  toUtc?: string;
  take?: number;
}

export interface SecurityPolicy {
  minPasswordLength: number;
  requireUppercase: boolean;
  requireDigit: boolean;
  requireNonAlphanumeric: boolean;
  requireMfa: boolean;
  sessionTimeoutMinutes: number;
  maxConcurrentSessions: number;
  ipAllowList: string[];
  geoAllowList: string[];
  apiKeyExpirationDays: number;
  enforceApiKeyRotation: boolean;
  failedLoginAlertThreshold: number;
}

export interface GovernancePolicy {
  allowedProviders: string[];
  allowedModels: string[];
  maximumGpuCostPerHour?: number | null;
  maximumRunningPods?: number | null;
  maximumQueueSize?: number | null;
  maximumDailySpendUsd?: number | null;
  allowedPlugins: string[];
  allowedMcpServers: string[];
  emptyAllowListMeansAllowAll: boolean;
}

export interface OrganizationPolicies {
  security: SecurityPolicy;
  governance: GovernancePolicy;
}

export interface UpdatePoliciesRequest {
  security?: SecurityPolicy | null;
  governance?: GovernancePolicy | null;
}

export interface ComplianceStatus {
  gdprEnabled: boolean;
  soc2Enabled: boolean;
  iso27001Enabled: boolean;
  dataRetentionDays: number;
  logRetentionDays: number;
  lastExportAt?: string | null;
  lastErasureAt?: string | null;
  overallStatus: string;
  controlChecklist: string[];
}

export interface ComplianceExportResult {
  jsonPayload: string;
  exportedAt: string;
}

export interface SessionInfo {
  id: string;
  userId: string;
  sessionId: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  startedAt: string;
  lastSeenAt: string;
  isActive: boolean;
}

export interface TrustedDevice {
  id: string;
  deviceName: string;
  lastIpAddress?: string | null;
  trustedAt: string;
  lastSeenAt: string;
  isRevoked: boolean;
}

export interface SecurityDashboard {
  securityScore: number;
  activeSessions: number;
  failedLogins24h: number;
  recentAuditEvents: number;
  secretCount: number;
  expiringSecrets: number;
  mfaCoveragePercent: number;
  complianceStatus: string;
  recentAudits: AuditEvent[];
}

export interface QuotaInfo {
  maxPods: number;
  maxProviders: number;
  maxModels: number;
  maxOrganizations: number;
  maxTeamMembers: number;
  maxApiRequestsPerMonth: number;
  maxConcurrentStreams: number;
  maxStorageGb: number;
}

export interface Plan {
  code: string;
  name: string;
  tier: string;
  pricingModel: string;
  monthlyPriceUsd: number;
  yearlyPriceUsd: number;
  seatPriceUsd: number;
  includedSeats: number;
  description?: string | null;
  quotas: QuotaInfo;
}

export interface Subscription {
  id: string;
  planCode: string;
  planName: string;
  status: string;
  billingInterval: string;
  paymentProvider?: string | null;
  seatCount: number;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  cancelAtPeriodEnd: boolean;
  quotas: QuotaInfo;
}

export interface StartCheckoutRequest {
  planCode: string;
  interval?: string;
  seatCount?: number;
  provider?: string;
  successUrl: string;
  cancelUrl: string;
}

export interface CheckoutSession {
  sessionId: string;
  checkoutUrl: string;
  provider: string;
}

export interface CancelSubscriptionRequest {
  atPeriodEnd?: boolean;
}

export interface UsageDashboard {
  periodStart: string;
  periodEnd: string;
  gpuHours: number;
  requests: number;
  tokens: number;
  bandwidthGb: number;
  storageGb: number;
  organizations: number;
  models: number;
  providers: number;
  estimatedMonthlyCostUsd: number;
  quotas: QuotaInfo;
  requestsQuotaPercent: number;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  status: string;
  subtotalUsd: number;
  taxUsd: number;
  totalUsd: number;
  periodStart: string;
  periodEnd: string;
  lineItemsJson: string;
}

export interface License {
  id: string;
  licenseKeyPrefix: string;
  edition: string;
  deploymentMode: string;
  isActivated: boolean;
  isValid: boolean;
  expiresAt?: string | null;
  maxSeats: number;
  lastValidatedAt?: string | null;
}

export interface ActivateLicenseRequest {
  licenseKey: string;
}

export interface IssueLicenseRequest {
  organizationId?: string | null;
  edition?: string;
  deploymentMode?: string;
  maxSeats?: number;
  expiresAt?: string | null;
}

export interface IssuedLicense {
  license: License;
  licenseKey: string;
}

export type OnboardingStepName =
  | 'CreateOrganization'
  | 'ConnectProvider'
  | 'CreatePod'
  | 'InstallOllama'
  | 'PullFirstModel'
  | 'ConnectClaudeCode'
  | 'TestAiGateway'
  | 'Completed';

export interface OnboardingStatus {
  currentStep: string;
  completedSteps: string[];
  isDismissed: boolean;
  isComplete: boolean;
}

export interface CompleteOnboardingStepRequest {
  step: string;
}

export interface TelemetryPreference {
  optIn: boolean;
  crashReports: boolean;
  performanceMetrics: boolean;
  featureUsage: boolean;
  healthReports: boolean;
}

export interface BackupJob {
  id: string;
  backupType: string;
  status: string;
  storageLocator?: string | null;
  sizeBytes?: number | null;
  startedAt?: string | null;
  completedAt?: string | null;
  errorMessage?: string | null;
  isScheduled: boolean;
}

export interface StartBackupRequest {
  backupType?: string;
  scheduled?: boolean;
}

export interface ReleaseStatus {
  currentVersion: string;
  latestVersion: string;
  updateAvailable: boolean;
  releaseNotes?: string | null;
  downloadUrl?: string | null;
  channel: string;
}

export interface CommercialDashboard {
  subscription: Subscription;
  usage: UsageDashboard;
  license: License;
  release: ReleaseStatus;
  estimatedMonthlyCostUsd: number;
  remainingRequestQuotaPercent: number;
}

export interface SystemComponentStatus {
  name: string;
  status: string;
}

export interface SystemStatus {
  status: string;
  version: string;
  updateAvailable: boolean;
  components: SystemComponentStatus[];
}

export interface DeploymentModel {
  id: string;
  modelReference: string;
  downloadStatus: string;
  progressPercent: number;
  isPrimary: boolean;
  errorMessage?: string | null;
}

export interface DeploymentLog {
  id: string;
  level: string;
  stage: string;
  message: string;
  timestampUtc: string;
}

export interface DeploymentHealth {
  state: string;
  gpuAvailable: boolean;
  cudaAvailable: boolean;
  runtimeRunning: boolean;
  modelAvailable: boolean;
  gatewayReachable: boolean;
  streamingWorks: boolean;
  lastCheckedAt?: string | null;
}

export interface Deployment {
  id: string;
  name: string;
  status: string;
  runtime: string;
  gpuCode: string;
  region: string;
  progressPercent: number;
  statusMessage?: string | null;
  healthState: string;
  estimatedHourlyCostUsd: number;
  createdAt: string;
  gpuPodId?: string | null;
  providerId: string;
  cloudProvider: string;
  cudaVersion: string;
  imageName?: string | null;
  errorMessage?: string | null;
  gatewayRouteId?: string | null;
  readyAt?: string | null;
  models: DeploymentModel[];
  logs: DeploymentLog[];
  health?: DeploymentHealth | null;
}

export interface GpuCatalogEntry {
  id: string;
  code: string;
  name: string;
  gpuType: string;
  vramGb: number;
  cudaCapability: string;
  estimatedHourlyCostUsd: number;
  providerAvailability: string[];
  isCustom: boolean;
}

export interface ModelCatalogEntry {
  id: string;
  code: string;
  modelReference: string;
  name: string;
  provider: string;
  version: string;
  family: string;
  parameters: string;
  quantization?: string | null;
  contextLength: number;
  requiredVramGb: number;
  recommendedGpuCode: string;
  minimumGpuCode: string;
  supportsVision: boolean;
  supportsTools: boolean;
  supportsEmbeddings: boolean;
  license?: string | null;
  downloadSizeGb: number;
  preferredRuntime: string;
}

export interface DeploymentTemplate {
  id: string;
  code: string;
  name: string;
  kind: string;
  description?: string | null;
  runtime: string;
  containerImage: string;
  recommendedGpuCode: string;
  defaultModelCodes: string[];
}

export interface DeploymentRegion {
  code: string;
  name: string;
  area: string;
  estimatedLatencyMs?: number | null;
  priceScore?: number | null;
  availabilityScore: number;
}

export interface GpuRecommendation {
  recommendedGpuCode: string;
  minimumGpuCode: string;
  requiredVramGb: number;
  estimatedPerformance: string;
  warnings: string[];
}

export interface DeploymentDashboard {
  runningDeployments: number;
  downloadingModels: number;
  healthyDeployments: number;
  failedDeployments: number;
  estimatedMonthlyCostUsd: number;
  recent: Deployment[];
}

export interface CreateDeploymentRequest {
  name: string;
  providerId: string;
  region: string;
  gpuCode: string;
  providerGpuId?: string | null;
  runtime: string;
  models: string[];
  templateCode?: string | null;
  environmentVariables?: Record<string, string> | null;
}

