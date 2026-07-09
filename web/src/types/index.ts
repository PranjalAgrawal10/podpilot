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
  MemberRead: 'Member.Read',
  MemberManage: 'Member.Manage',
  MemberRoleUpdate: 'Member.RoleUpdate',
  InvitationCreate: 'Invitation.Create',
  OrchestratorRead: 'Orchestrator.Read',
  OrchestratorManage: 'Orchestrator.Manage',
  ObservabilityRead: 'Observability.Read',
  ObservabilityExport: 'Observability.Export',
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
