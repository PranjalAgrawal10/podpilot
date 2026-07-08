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
  ModelPull: 'Model.Pull',
  ModelDelete: 'Model.Delete',
  DashboardView: 'Dashboard.View',
  BillingView: 'Billing.View',
  MemberRead: 'Member.Read',
  MemberManage: 'Member.Manage',
  MemberRoleUpdate: 'Member.RoleUpdate',
  InvitationCreate: 'Invitation.Create',
} as const;

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
